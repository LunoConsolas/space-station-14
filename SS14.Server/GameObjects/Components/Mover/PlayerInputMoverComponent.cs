﻿using SS14.Server.Interfaces.GameObjects;
using SS14.Shared.GameObjects;
using SS14.Shared.Log;
using SS14.Shared.Maths;
using SS14.Shared.Serialization;

namespace SS14.Server.GameObjects
{
    /// <summary>
    ///     Moves the entity based on input from a KeyBindingInputComponent.
    /// </summary>
    public class PlayerInputMoverComponent : Component, IMoverComponent
    {
        private bool _movingUp;
        private bool _movingDown;
        private bool _movingLeft;
        private bool _movingRight;

        /// <inheritdoc />
        public override string Name => "PlayerInputMover";

        /// <inheritdoc />
        public override uint? NetID => null;

        /// <inheritdoc />
        public override bool NetworkSynchronizeExistence => false;

        /// <summary>
        ///     Movement speed (m/s) that the entity walks.
        /// </summary>
        public float WalkMoveSpeed { get; set; } = 4.0f;

        /// <summary>
        ///     Movement speed (m/s) that the entity sprints.
        /// </summary>
        public float SprintMoveSpeed { get; set; } = 10.0f;

        /// <summary>
        ///     Is the entity Sprinting (running)?
        /// </summary>
        public bool Sprinting { get; set; }

        /// <summary>
        ///     Calculated linear velocity direction of the entity.
        /// </summary>
        public Vector2 VelocityDir { get; private set; }

        /// <inheritdoc />
        public override void OnAdd()
        {
            // This component requires that the entity has a PhysicsComponent.
            if (!Owner.HasComponent<PhysicsComponent>())
                Logger.Error($"[ECS] {Owner.Prototype.Name} - {nameof(PlayerInputMoverComponent)} requires {nameof(PhysicsComponent)}. ");

            base.OnAdd();
        }

        /// <inheritdoc />
        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataReadWriteFunction("wspd", 4.0f, value => WalkMoveSpeed = value, () => WalkMoveSpeed);
            serializer.DataReadWriteFunction("rspd", 10.0f, value => SprintMoveSpeed = value, () => SprintMoveSpeed);

            // The velocity and moving directions is usually set from player or AI input,
            // so we don't want to save/load these derived fields.
        }

        /// <summary>
        ///     Toggles one of the four cardinal directions. Each of the four directions are
        ///     composed into a single direction vector, <see cref="VelocityDir"/>. Enabling
        ///     opposite directions will cancel each other out, resulting in no direction.
        /// </summary>
        /// <param name="direction">Direction to toggle.</param>
        /// <param name="enabled">If the direction is active.</param>
        public void SetVelocityDirection(Direction direction, bool enabled)
        {
            switch (direction)
            {
                case Direction.East:
                    _movingRight = enabled;
                    break;
                case Direction.North:
                    _movingUp = enabled;
                    break;
                case Direction.West:
                    _movingLeft = enabled;
                    break;
                case Direction.South:
                    _movingDown = enabled;
                    break;
            }

            // key directions are in screen coordinates
            // _moveDir is in world coordinates
            // if the camera is moved, this needs to be changed

            var x = 0;
            x -= _movingLeft ? 1 : 0;
            x += _movingRight ? 1 : 0;

            var y = 0;
            y += _movingDown ? 1 : 0;
            y -= _movingUp ? 1 : 0;

            VelocityDir = new Vector2(x, y);

            // can't normalize zero length vector
            if (VelocityDir.LengthSquared > 1.0e-6)
                VelocityDir = VelocityDir.Normalized;
        }
    }
}
