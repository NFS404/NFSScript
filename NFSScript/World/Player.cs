﻿using System;
using static NFSScript.Core.GameMemory;
using static NFSScript.Core.WorldAddresses;
using static NFSScript.World.EASharpBindings;
using NFSScript.Math;

namespace NFSScript.World
{
    /// <summary>
    /// A class that represents the game's <see cref="Player"/>.
    /// </summary>
    public static class Player
    {
        /// <summary>
        /// Returns the current status of the player.
        /// </summary>
        public static PlayerStatus Status
        {
            get
            {
                return (PlayerStatus)memory.ReadInt32((IntPtr)memory.getBaseAddress + GameAddrs.NON_STATIC_PLAYER_STATUS);
            }
        }

        /// <summary>
        /// Returns true if the <see cref="Player"/> is in free roam.
        /// </summary>
        public static bool IsFreeRoam
        {
            get
            {
                return memory.ReadByte((IntPtr)memory.getBaseAddress + GameAddrs.NON_STATIC_IS_FREE_ROAM) == 1;
            }
        }

        /// <summary>
        /// <see cref="Player"/>'s cash (Inaccurate read only value).
        /// </summary>
        public static int Cash
        {
            get
            {
                return memory.ReadInt32((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_PLAYER_CASH);
            }
        }

        /// <summary>
        /// <see cref="Player"/>'s boost (Inaccurate read only value).
        /// </summary>
        public static int Boost
        {
            get
            {
                return memory.ReadInt32((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_PLAYER_BOOST);
            }
        }

        /// <summary>
        /// Returns the currnet amount of gems that the player has.
        /// </summary>
        public static int CurrentAmountOfGems
        {
            get
            {
                int address = memory.ReadInt32((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_GEMS_COLLECTED);
                address = memory.ReadInt32((IntPtr)address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_1);
                address = memory.ReadInt32((IntPtr)address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_2);
                address = memory.ReadInt32((IntPtr)address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_3);
                address = memory.ReadInt32((IntPtr)address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_4);
                address = memory.ReadInt32((IntPtr)address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_5);

                return memory.ReadInt32((IntPtr)address);
            }
        }

        /// <summary>
        /// Disables auto-drive.
        /// </summary>
        public static void DisableAutoDrive()
        {
            memory.WriteByte((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_AUTODRIVE, 0);
        }

        /// <summary>
        /// Changes the in-game auto-drive function state.
        /// </summary>
        /// <remarks>
        /// Requires any kind of directional input in-game in order to apply the change.
        /// </remarks>
        public static void ChangeAutoDrive(bool enableAutoDrive)
        {
            if(enableAutoDrive)
                memory.WriteByte((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_AUTODRIVE, 1);
            else
                memory.WriteByte((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_AUTODRIVE, 0);

        }

        /// <summary>
        /// A class that represents the <see cref="Player"/>'s car.
        /// </summary>
        public static class Car
        {
            /// <summary>
            /// The <see cref="Player"/>'s car position.
            /// </summary>
            public static Vector3 Position
            {
                get
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    float x = memory.ReadFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_X_POS);
                    float y = memory.ReadFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_Y_POS);
                    float z = memory.ReadFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_Z_POS);

                    return new Vector3(x, y, z);
                }
                set
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    memory.WriteFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_X_POS, value.X);
                    memory.WriteFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_Y_POS, value.Y);
                    memory.WriteFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_Z_POS, value.Z);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car rotation.
            /// </summary>
            public static Quaternion Rotation
            {
                get
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    float q1 = memory.ReadFloat((IntPtr)(addr + (GameAddrs.PSTATIC_CAR_ANGULAR_VELOCITY + 0x5C)));
                    float q2 = memory.ReadFloat((IntPtr)(addr + (GameAddrs.PSTATIC_CAR_FACING_SOUTH)));
                    float q3 = memory.ReadFloat((IntPtr)(addr + (GameAddrs.PSTATIC_CAR_FACING_UP)));
                    float q4 = memory.ReadFloat((IntPtr)(addr + (GameAddrs.PSTATIC_CAR_FACING_EAST)));

                    return new Quaternion(q1, q2, q3, q4);
                }
            }

            /// <summary>
            /// Gets the local player's memory offset relative to <see cref="Game.PWorld_Cars"/>.
            /// </summary>
            public static int CarOffset
            {
                get
                {
                    for (int i = 0; i < 0x78; i++)
                    {
                        int offset = 0xB0 * i;
                        int addr = Game.PWorld_Cars + offset;

                        // This check can be improved with bytes from offset +0x5C to +0x60.
                        // 0x5D is non-player-checkish, 0x5C is initialized-checkish, check game code for more info.
                        float localX = memory.ReadFloat((IntPtr)(memory.getBaseAddress + PlayerAddrs.NON_STATIC_PLAYER_X_POS));
                        float localY = memory.ReadFloat((IntPtr)(memory.getBaseAddress + PlayerAddrs.NON_STATIC_PLAYER_Y_POS));
                        float localZ = memory.ReadFloat((IntPtr)(memory.getBaseAddress + PlayerAddrs.NON_STATIC_PLAYER_Z_POS));
                        float opponentX = memory.ReadFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_X_POS));
                        float opponentY = -memory.ReadFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_Y_POS)); // the - at the start is important
                        float opponentZ = memory.ReadFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_Z_POS));

                        if (Mathf.Abs(localX - opponentX) < 1f &&
                            Mathf.Abs(localY - opponentY) < 1f &&
                            Mathf.Abs(localZ - opponentZ) < 1f)
                        {
                            return offset;
                        }
                    }
                    return 0xFF;
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car speed in MPS, you need to set it in MPS as well.
            /// </summary>
            /// <remarks>
            /// See <see cref="Math.Mathf.ConvertSpeed(float, SpeedMeasurementConversionTypes)"/>.
            /// </remarks>
            public static float Speed
            {
                get
                {
                    int address = memory.ReadInt32((IntPtr)memory.getBaseAddress + 0x91F9D0);
                    address = memory.ReadInt32((IntPtr)address + 0x68);
                    
                    return memory.ReadFloat((IntPtr)address);
                }
                set
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    float southMult = memory.ReadFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_FACING_SOUTH);
                    float vertMult = memory.ReadFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_FACING_UP);
                    float eastMult = memory.ReadFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_FACING_EAST);

                    memory.WriteFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_SOUTH, southMult * value);
                    memory.WriteFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_VERTICAL_VELOCITY, vertMult * value);
                    memory.WriteFloat((IntPtr)addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_EAST, eastMult * value);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car's gravity.
            /// </summary>
            /// <remarks>
            /// Default value is 1000.
            /// </remarks>
            public static float Gravity
            {
                get
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    return memory.ReadFloat((IntPtr)(addr +GameAddrs.PSTATIC_CAR_GRAVITY));
                }
                set
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    memory.WriteFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_GRAVITY), value);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car weight.
            /// </summary>
            /// <remarks>
            /// Each car has its own weight.
            /// </remarks>
            public static float Weight
            {
                get
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    return memory.ReadFloat((IntPtr)(addr +GameAddrs.PSTATIC_CAR_WEIGHT));
                }
                set
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    memory.WriteFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_WEIGHT), value);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car current velocity towards east.
            /// </summary>
            public static float VelocityTowardsEast
            {
                get
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    return memory.ReadFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_EAST));
                }
                set
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    memory.WriteFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_EAST), value);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car current velocity towards south.
            /// </summary>xw
            public static float VelocityTowardsSouth
            {
                get
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    return memory.ReadFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_SOUTH));
                }
                set
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    memory.WriteFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_SOUTH), value);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car vertical velocity towards sky.
            /// </summary>
            public static float VerticalVelocity
            {
                get
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    return memory.ReadFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_VERTICAL_VELOCITY));
                }
                set
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    memory.WriteFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_VERTICAL_VELOCITY), value);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car angular velocity towards right.
            /// </summary>
            public static float AngularVelocity
            {
                get
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    return memory.ReadFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_ANGULAR_VELOCITY));
                }
                set
                {
                    int addr = Game.PWorld_Cars + CarOffset;
                    memory.WriteFloat((IntPtr)(addr + GameAddrs.PSTATIC_CAR_ANGULAR_VELOCITY), value);
                }
            }

            /// <summary>
            /// Instantly stops the <see cref="Player"/>'s car.
            /// </summary>
            public static void ForceStop()
            {
                VelocityTowardsSouth = 0;
                VelocityTowardsEast = 0;
            }

            /// <summary>
            /// Pushes the <see cref="Player"/>'s car to north.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void PushNorth(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    VelocityTowardsSouth = 0f;
                VelocityTowardsSouth -= amountOfForce;
            }

            /// <summary>
            /// Pushes the <see cref="Player"/>'s car to west.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void PushWest(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    VelocityTowardsEast = 0f;
                VelocityTowardsEast -= amountOfForce;
            }

            /// <summary>
            /// Pushes the <see cref="Player"/>'s car to east.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void PushEast(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    VelocityTowardsEast = 0f;
                VelocityTowardsEast += amountOfForce;
            }

            /// <summary>
            /// Pushes the <see cref="Player"/>'s car to south.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void PushSouth(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    VelocityTowardsSouth = 0f;
                VelocityTowardsSouth += amountOfForce;
            }

            /// <summary>
            /// 'Member Mario? I 'member!
            /// </summary>
            public static void Hop()
            {
                VerticalVelocity = 10f;
            }

            /// <summary>
            /// Pushes the <see cref="Player"/>'s car above.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void ForceJump(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    VerticalVelocity = 0f;
                VerticalVelocity += amountOfForce;
            }

            /// <summary>
            /// Applies a force that turns the <see cref="Player"/>'s car clockwise.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void TurnClockwise(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    AngularVelocity = 0f;
                AngularVelocity += amountOfForce;
            }

            /// <summary>
            /// Applies a force that turns the <see cref="Player"/>'s car counter-clockwise.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void TurnCounterClockwise(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    AngularVelocity = 0f;
                AngularVelocity -= amountOfForce;
            }

            /// <summary>
            /// Disables collision with walls.
            /// </summary>
            public static void DisableWallCollisions()
            {
                memory.WriteByte((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_WALL_COLLISION, 0x38);
            }

            /// <summary>
            /// Enables collision with walls.
            /// </summary>
            public static void EnableWallCollisions()
            {
                memory.WriteByte((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_WALL_COLLISION, 0x84);
            }

            /// <summary>
            /// Disables car collision.
            /// </summary>
            public static void DisableCarCollision()
            {
                memory.WriteByte((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_CAR_COLLISION, 0xEB);
            }

            /// <summary>
            /// Enables car collision.
            /// </summary>
            public static void EnableCarCollision()
            {
                memory.WriteByte((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_CAR_COLLISION, 0x74);
            }
        }
    }

    /// <summary>
    /// A class for powerups.
    /// </summary>
    public static class Powerups
    {
        /// <summary>
        /// Changes the current powerup configuration/deck.
        /// </summary>
        /// <param name="config"></param>
        public static void AssignPowerupConfiguration(PowerupConfiguration config)
        {
            CallBinding(_EASharpBinding_637, (int)config);
        }

        /// <summary>
        /// Recharges all the powerups.
        /// </summary>
        public static void RechargeAllPowerups()
        {
            CallBinding(_EASharpBinding_638);
        }

        /// <summary>
        /// Enables powerup cooldown.
        /// </summary>
        public static void EnablePowerupCooldown()
        {
            memory.WriteByteArray((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_POWERUP_COOLDOWN, new byte[] { 0x80, 0x7D, 0xFB, 0x0 });
        }

        /// <summary>
        /// Blocks powerups from going into cooldown.
        /// </summary>
        public static void DisablePowerupCooldown()
        {
            memory.WriteByteArray((IntPtr)memory.getBaseAddress + PlayerAddrs.NON_STATIC_POWERUP_COOLDOWN, new byte[] { 0x3A, 0xC0, 0x90, 0x90 });
        }
    }
}
