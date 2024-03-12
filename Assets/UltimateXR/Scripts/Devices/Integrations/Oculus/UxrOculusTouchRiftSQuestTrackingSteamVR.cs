﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrOculusTouchRiftSQuestTrackingSteamVR.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Devices.Integrations.SteamVR;

namespace UltimateXR.Devices.Integrations.Oculus
{
    /// <summary>
    ///     Tracking for Oculus Touch controllers using SteamVR SDK.
    /// </summary>
    public class UxrOculusTouchRiftSQuestTrackingSteamVR : UxrSteamVRControllerTracking
    {
        #region Public Overrides UxrControllerTracking

        /// <inheritdoc />
        public override Type RelatedControllerInputType => typeof(UxrOculusTouchRiftSQuestInputSteamVR);

        #endregion
    }
}