// Guids.cs
// MUST match guids.h
using System;

namespace jla.SpotifyVSExtension
{
    static class GuidList
    {
        public const string guidSpotifyVSExtensionPkgString = "6374d2dd-92ca-4def-b351-49e09adeef6d";
        public const string guidSpotifyVSExtensionCmdSetString = "9beed4ba-77e2-4fc1-b84b-93a5435cfdbc";
        public const string guidToolWindowPersistanceString = "79401bf2-c238-431c-811f-78bbab7e16e2";

        public static readonly Guid guidSpotifyVSExtensionCmdSet = new Guid(guidSpotifyVSExtensionCmdSetString);
    };
}