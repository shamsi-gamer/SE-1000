
namespace IngameScript
{
    partial class Program
    {
        void InitMixerLabels()
        { 
            lblMixerVolumeUp   = new Label(Lbl("Volume Up"));
            lblMixerVolumeDown = new Label(Lbl("Volume Down"));
            lblMixerAll        = new Label(Lbl("Solo"), lbl => g_session.EditClip == 1, lbl => OK(g_session.ClipCopy), UpdateMixerAll);
            lblMixerMuteAll    = new Label(Lbl("Mute"), lbl => g_session.EditClip == 2, null,                          UpdateMixerMuteAll);

            lblMixerShift      = new Label(Lbl("M Shift"), lbl => EditedClip.MixerShift);

            lblSession = new Label(Lbl("Session"), 
                lbl => 
                       g_session.ShowSession
                    && g_session.EditClip == 0, 
                null, 
                UpdateSessionLabel, 
                null, 
                0, 
                F, 
                T);
        }


        void UpdateMixerAll(Label lbl)
        {
            if (g_session.ShowSession) lbl.SetText("Dup");
            else                       lbl.SetText("Solo", 8, 18);
        }


        void UpdateMixerMuteAll(Label lbl)
        {
            if (g_session.ShowSession) lbl.SetText("Del");
            else                       lbl.SetText("Mute", 8, 18);
        }
    }
}
