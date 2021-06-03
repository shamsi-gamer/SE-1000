
namespace IngameScript
{
    partial class Program
    {
        void InitMixerLabels()
        { 
            lblMixerVolumeUp = new Label(0, GetLabel("Volume Up"),
                lbl => !ShowClip && EditClip == 0, 
                CF_null,
                UpdateVolumeUpAll);

            lblMixerVolumeDown = new Label(0, GetLabel("Volume Down"), 
                lbl => !ShowClip && EditClip == 1 && !OK(ClipCopy), 
                lbl => !ShowClip && EditClip == 1 &&  OK(ClipCopy), 
                UpdateVolumeDownAll);

            lblMixerAll = new Label(0, GetLabel("Solo"),
                lbl => !ShowClip && EditClip == 2 && !OK(ClipCopy), 
                lbl => !ShowClip && EditClip == 2 &&  OK(ClipCopy), 
                UpdateMixerAll);

            lblMixerMuteAll = new Label(0, GetLabel("Mute"),
                lbl => !ShowClip && EditClip == 3, 
                CF_null,             
                UpdateMixerMuteAll);

            lblMixerShift = new Label(0, GetLabel("M Shift"),   lbl => MixerShift);

            lblShowClip   = new Label(0, GetLabel("Show Clip"), lbl => ShowClip);
            lblMix        = new Label(0, GetLabel("Mix"),       lbl => ShowMixer);
            lblCueClip    = new Label(0, GetLabel("Cue Clip"),  lbl => CueClip);
        }


        void UpdateVolumeUpAll(Label lbl)
        {
            if (   ShowMixer
                || ShowClip) lbl.SetText("Vol ►", 8, 18);
            else             lbl.SetText("Set");
        }


        void UpdateVolumeDownAll(Label lbl)
        {
            if (   ShowMixer
                || ShowClip) lbl.SetText("◄ Vol", 8, 18);
            else             lbl.SetText("Move");
        }


        void UpdateMixerAll(Label lbl)
        {
            if (   ShowMixer
                || ShowClip) lbl.SetText("Solo", 8, 18);
            else             lbl.SetText("Dup");
        }


        void UpdateMixerMuteAll(Label lbl)
        {
                 if ( ShowMixer) lbl.SetText(ShowClip ? strEmpty : "Mute", 8, 18);
            else if (!ShowClip ) lbl.SetText("Del");
        }
    }
}
