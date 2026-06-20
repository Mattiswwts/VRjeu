namespace ActionGame.Networking
{
    /// <summary>
    /// Utilitaire partagé — dit si le jeu tourne en VR.
    /// Android (Quest) = toujours VR.
    /// PC Editor/Build = VR seulement si un casque est branché et actif.
    /// </summary>
    public static class XRCheck
    {
        public static bool IsVR()
        {
#if UNITY_ANDROID
            return true;
#else
            return UnityEngine.XR.XRSettings.isDeviceActive;
#endif
        }
    }
}
