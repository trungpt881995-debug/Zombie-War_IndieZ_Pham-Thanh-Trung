using System;
using NUnit.Framework;
using ZombieWar.Features.Control.Controller;
using ZombieWar.Features.Control.Domain;
using ZombieWar.Features.Control.Input;
using ZombieWar.Features.Control.Model;
using ZombieWar.Features.Control.Ports;
using ZombieWar.Features.Control.View;

namespace ZombieWar.Features.Control.Tests
{
    public sealed class ControlFeatureTests
    {
        private sealed class SpyMovementSink : IMovementIntentSink
        {
            public int SetCount { get; private set; }
            public MovementIntent Last { get; private set; }

            public void Set(in MovementIntent intent)
            {
                SetCount++;
                Last = intent;
            }
        }

        private sealed class FakeControlView : IControlView
        {
            public event Action<ControlPointerSample> PointerDownRequested;
            public event Action<ControlPointerSample> PointerDragged;
            public event Action<int> PointerUpRequested;
            public event Action CancelRequested;

            public int ShowCount { get; private set; }
            public int HideCount { get; private set; }
            public int HandleCount { get; private set; }
            public float LastShowX { get; private set; }
            public float LastShowY { get; private set; }
            public float LastHandleX { get; private set; }
            public float LastHandleY { get; private set; }

            public void ShowAt(float localX, float localY)
            {
                ShowCount++;
                LastShowX = localX;
                LastShowY = localY;
            }

            public void SetHandleOffset(float x, float y)
            {
                HandleCount++;
                LastHandleX = x;
                LastHandleY = y;
            }

            public void Hide() => HideCount++;

            public void Down(int id, float x, float y) => PointerDownRequested?.Invoke(new ControlPointerSample(id, x, y));

            public void Drag(int id, float x, float y) => PointerDragged?.Invoke(new ControlPointerSample(id, x, y));

            public void Up(int id) => PointerUpRequested?.Invoke(id);
            public void Cancel() => CancelRequested?.Invoke();
        }

        private static DynamicJoystickModel CreateModel(float deadZone = 0.2f, float maxRadius = 100f, float sensitivity = 1f)
        {
            return new DynamicJoystickModel(new JoystickSettings(deadZone, maxRadius, sensitivity));
        }

        [Test]
        public void Model_Begin_CapturesPointerAndEntersTracking()
        {
            var model = CreateModel();
            var pointer = new ControlPointerSample(7, 10f, 20f);

            bool accepted = model.Begin(in pointer);

            Assert.IsTrue(accepted);
            Assert.AreEqual(ControlState.Tracking, model.State);
            Assert.AreEqual(7, model.ActivePointerId);
        }

        [Test]
        public void Model_SecondPointerBegin_IsRejected()
        {
            var model = CreateModel();
            var first = new ControlPointerSample(1, 0f, 0f);
            var second = new ControlPointerSample(2, 10f, 10f);
            model.Begin(in first);

            bool accepted = model.Begin(in second);

            Assert.IsFalse(accepted);
            Assert.AreEqual(1, model.ActivePointerId);
        }

        [Test]
        public void Model_DragInsideDeadZone_ReturnsZeroIntent()
        {
            var model = CreateModel(deadZone: 0.2f, maxRadius: 100f);
            var down = new ControlPointerSample(1, 0f, 0f);
            model.Begin(in down);
            var drag = new ControlPointerSample(1, 10f, 0f); // 0.10 raw magnitude

            var result = model.Update(in drag);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(10f, result.HandleOffsetX, 0.0001f);
            Assert.IsFalse(result.Intent.HasInput);
            Assert.AreEqual(0f, result.Intent.Magnitude);
        }

        [Test]
        public void Model_DragOutsideDeadZone_RemapsMagnitude()
        {
            var model = CreateModel(deadZone: 0.2f, maxRadius: 100f, sensitivity: 1f);
            var down = new ControlPointerSample(1, 0f, 0f);
            model.Begin(in down);
            var drag = new ControlPointerSample(1, 60f, 0f); // (0.6 - 0.2) / 0.8 = 0.5

            var result = model.Update(in drag);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(60f, result.HandleOffsetX, 0.0001f);
            Assert.AreEqual(0.5f, result.Intent.X, 0.0001f);
            Assert.AreEqual(0f, result.Intent.Y, 0.0001f);
            Assert.AreEqual(0.5f, result.Intent.Magnitude, 0.0001f);
        }

        [Test]
        public void Model_DragBeyondMaxRadius_ClampsHandleAndIntent()
        {
            var model = CreateModel(deadZone: 0f, maxRadius: 100f, sensitivity: 1f);
            var down = new ControlPointerSample(1, 0f, 0f);
            model.Begin(in down);
            var drag = new ControlPointerSample(1, 250f, 0f);

            var result = model.Update(in drag);

            Assert.AreEqual(100f, result.HandleOffsetX, 0.0001f);
            Assert.AreEqual(1f, result.Intent.X, 0.0001f);
            Assert.AreEqual(1f, result.Intent.Magnitude, 0.0001f);
        }

        [Test]
        public void Model_WrongPointerDragAndUp_AreIgnored()
        {
            var model = CreateModel();
            var down = new ControlPointerSample(5, 0f, 0f);
            model.Begin(in down);
            var wrongDrag = new ControlPointerSample(6, 100f, 0f);

            var result = model.Update(in wrongDrag);
            bool ended = model.End(6);

            Assert.IsFalse(result.Accepted);
            Assert.IsFalse(ended);
            Assert.AreEqual(ControlState.Tracking, model.State);
            Assert.AreEqual(5, model.ActivePointerId);
        }

        [Test]
        public void Model_CorrectPointerUp_ReturnsToIdle()
        {
            var model = CreateModel();
            var down = new ControlPointerSample(5, 0f, 0f);
            model.Begin(in down);

            bool ended = model.End(5);

            Assert.IsTrue(ended);
            Assert.AreEqual(ControlState.Idle, model.State);
            Assert.AreEqual(-1, model.ActivePointerId);
        }

        [Test]
        public void Controller_EnabledInput_DrivesMovementAndStopsOnPointerUp()
        {
            var model = CreateModel(deadZone: 0f, maxRadius: 100f);
            var view = new FakeControlView();
            var gate = new GameplayInputGate(true);
            var sink = new SpyMovementSink();
            using var controller = new ControlController(model, view, gate, sink);

            view.Down(1, 10f, 20f);
            view.Drag(1, 110f, 20f);

            Assert.AreEqual(1, view.ShowCount);
            Assert.AreEqual(10f, view.LastShowX);
            Assert.AreEqual(20f, view.LastShowY);
            Assert.AreEqual(1f, sink.Last.Magnitude, 0.0001f);

            view.Up(1);

            Assert.IsFalse(sink.Last.HasInput);
            Assert.AreEqual(ControlState.Idle, model.State);
            Assert.GreaterOrEqual(view.HideCount, 2); // constructor + pointer up
        }

        [Test]
        public void Controller_DisabledInput_IgnoresPointerDown()
        {
            var model = CreateModel();
            var view = new FakeControlView();
            var gate = new GameplayInputGate(false);
            var sink = new SpyMovementSink();
            using var controller = new ControlController(model, view, gate, sink);

            view.Down(1, 10f, 20f);

            Assert.AreEqual(ControlState.Idle, model.State);
            Assert.AreEqual(0, view.ShowCount);
        }

        [Test]
        public void Controller_DisablingInputWhileTracking_CancelsAndSendsZero()
        {
            var model = CreateModel(deadZone: 0f, maxRadius: 100f);
            var view = new FakeControlView();
            var gate = new GameplayInputGate(true);
            var sink = new SpyMovementSink();
            using var controller = new ControlController(model, view, gate, sink);

            view.Down(1, 0f, 0f);
            view.Drag(1, 100f, 0f);
            Assert.IsTrue(sink.Last.HasInput);

            gate.SetGameplayInputEnabled(false);

            Assert.AreEqual(ControlState.Idle, model.State);
            Assert.IsFalse(sink.Last.HasInput);
        }
    }
}
