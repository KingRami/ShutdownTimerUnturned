// Decompiled with JetBrains decompiler
// Type: ShutdownTimer.PluginConfig
// Assembly: ShutdownTimer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 777D4D7A-8747-4E92-A090-092FC80FAED8
// Assembly location: E:\PycharmProjects\Bot-Projekte\unturned\ShutdownTimer.dll

using Rocket.API;
using System.Collections.Generic;
using System.Linq;

namespace ShutdownTimer
{
    public class PluginConfig : IRocketPluginConfiguration, IDefaultable
    {
        public bool UseRichText;
        public string MessageIcon;
        public string AnnounceTimes;

        public void LoadDefaults()
        {
            this.UseRichText = true;
            this.MessageIcon = "https://i.imgur.com/bIaTidU.png";
            this.AnnounceTimes = "30,15,10,5,4,3,2,1";

        }
    }
}
