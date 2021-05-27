
namespace IngameScript
{
    partial class Program
    {
        void InitMixerLabels()
        { 
            lblMixerVolumeUp   = new Label(GetLabel("Volume Up"));
            lblMixerVolumeDown = new Label(GetLabel("Volume Down"));
            lblMixerAll        = new Label(GetLabel("Solo"), lbl => EditClip == 1, lbl => OK(ClipCopy), UpdateMixerAll);
            lblMixerMuteAll    = new Label(GetLabel("Mute"), lbl => EditClip == 2, null,                          UpdateMixerMuteAll);

            lblMixerShift      = new Label(GetLabel("M Shift"), lbl => MixerShift);

            lblSession = new Label(GetLabel("Session"), 
                lbl => 
                       ShowSession
                    && EditClip == 0, 
                null, 
                UpdateSessionLabel, 
                null, 
                0, 
                F, 
                T);
        }


        void UpdateMixerAll(Label lbl)
        {
            if (ShowSession) lbl.SetText("Dup");
            else             lbl.SetText("Solo", 8, 18);
        }


        void UpdateMixerMuteAll(Label lbl)
        {
            if (ShowSession) lbl.SetText("Del");
            else             lbl.SetText("Mute", 8, 18);
        }
    }
}
