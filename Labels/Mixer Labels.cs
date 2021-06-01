
namespace IngameScript
{
    partial class Program
    {
        void InitMixerLabels()
        { 
            lblMixerVolumeUp = new Label(0, GetLabel("Volume Up"), CF_null, CF_null, UpdateVolumeUpAll);

            lblMixerVolumeDown = new Label(0, GetLabel("Volume Down"), 
                lbl => EditClip == 0 && !OK(ClipCopy), 
                lbl => EditClip == 0 &&  OK(ClipCopy), 
                UpdateVolumeDownAll);

            lblMixerAll = new Label(0, GetLabel("Solo"),
                lbl => EditClip == 1 && !OK(ClipCopy), 
                lbl => EditClip == 1 &&  OK(ClipCopy), 
                UpdateMixerAll);

            lblMixerMuteAll = new Label(0, GetLabel("Mute"),
                lbl => EditClip == 2, 
                CF_null,             
                UpdateMixerMuteAll);

            lblMixerShift = new Label(0, GetLabel("M Shift"),   lbl => MixerShift);

            lblShowClip   = new Label(0, GetLabel("Show Clip"), lbl => ShowClip);
            lblMix        = new Label(0, GetLabel("Mix"),       lbl => ShowMixer);
            lblCueClip    = new Label(0, GetLabel("Cue Clip"),  lbl => CueClip);

        }


        void UpdateVolumeUpAll(Label lbl)
        {
            if (!ShowMixer) lbl.SetText("Scn");
            else            lbl.SetText("Vol ►", 8, 18);
        }


        void UpdateVolumeDownAll(Label lbl)
        {
            if (!ShowMixer) lbl.SetText("Move");
            else            lbl.SetText("◄ Vol", 8, 18);
        }


        void UpdateMixerAll(Label lbl)
        {
            if (!ShowMixer) lbl.SetText("Dup");
            else            lbl.SetText("Solo", 8, 18);
        }


        void UpdateMixerMuteAll(Label lbl)
        {
            if (!ShowMixer) lbl.SetText("Del");
            else            lbl.SetText("Mute", 8, 18);
        }
    }
}
