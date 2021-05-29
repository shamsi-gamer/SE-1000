
namespace IngameScript
{
    partial class Program
    {
        void InitMixerLabels()
        { 
            lblMixerVolumeUp = new Label(GetLabel("Volume Up"), CF_null, CF_null, UpdateVolumeUpAll);

            lblMixerVolumeDown = new Label(GetLabel("Volume Down"), 
                lbl => EditClip == 1 && !OK(ClipCopy), 
                lbl => EditClip == 1 &&  OK(ClipCopy), 
                UpdateVolumeDownAll);

            lblMixerAll = new Label(GetLabel("Solo"),
                lbl => EditClip == 2 && !OK(ClipCopy), 
                lbl => EditClip == 2 &&  OK(ClipCopy), 
                UpdateMixerAll);

            lblMixerMuteAll = new Label(GetLabel("Mute"),
                lbl => EditClip == 3, 
                CF_null,             
                UpdateMixerMuteAll);

            lblCue        = new Label(GetLabel("Cue Clip"), lbl => CueClip);
            lblMixerShift = new Label(GetLabel("M Shift"),  lbl => MixerShift);

            lblSession = new Label(GetLabel("Session"), 
                lbl => 
                       ShowSession
                    && EditClip == 0, 
                CF_null, 
                UpdateSessionLabel, 
                AL_null, 
                0, 
                False, 
                True);
        }


        void UpdateVolumeUpAll(Label lbl)
        {
            if (ShowSession) lbl.SetText("Scn");
            else lbl.SetText("Vol ►", 8, 18);
        }


        void UpdateVolumeDownAll(Label lbl)
        {
            if (ShowSession) lbl.SetText("Move");
            else lbl.SetText("◄ Vol", 8, 18);
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
