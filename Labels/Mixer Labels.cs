
namespace IngameScript
{
    partial class Program
    {
        void InitMixerLabels()
        { 
            lblMixerVolumeUp = new Label(0, GetLabel("Volume Up"),
                lbl => ShowMixer == 0 && EditClip == 0, 
                CF_null,
                UpdateVolumeUpAll);

            lblMixerVolumeDown = new Label(0, GetLabel("Volume Down"), 
                lbl => ShowMixer == 0 && EditClip == 1 && !OK(ClipCopy), 
                lbl => ShowMixer == 0 && EditClip == 1 &&  OK(ClipCopy), 
                UpdateVolumeDownAll);

            lblMixerAll = new Label(0, GetLabel("Solo"),
                lbl => ShowMixer == 0 && EditClip == 2 && !OK(ClipCopy), 
                lbl => ShowMixer == 0 && (EditClip == 0 || EditClip == 2) &&  OK(ClipCopy), 
                UpdateMixerAll);

            lblMixerMuteAll = new Label(0, GetLabel("Mute"),
                lbl => ShowMixer == 0 && EditClip == 3, 
                CF_null,             
                UpdateMixerMuteAll);

            lblMixerShift = new Label(0, GetLabel("M Shift"),   lbl => MixerShift);

            lblShowClip   = new Label(0, GetLabel("Show Clip"), lbl => ShowClip);
            lblMix        = new Label(0, GetLabel("Mix"),       lbl => ShowMixer == 2, lbl => ShowMixer == 1);
            lblCueClip    = new Label(0, GetLabel("Cue Clip"),  lbl => CueClip   == 2, lbl => CueClip   == 1);
        }


        void UpdateVolumeUpAll(Label lbl)
        {
            if (ShowMixer > 0) lbl.SetText("Vol ►", 8, 18);
            else               lbl.SetText("Set");
        }


        void UpdateVolumeDownAll(Label lbl)
        {
            if (ShowMixer > 0) lbl.SetText("◄ Vol", 8, 18);
            else               lbl.SetText("Move");
        }


        void UpdateMixerAll(Label lbl)
        {
            if (ShowMixer > 0) lbl.SetText(ShowMixer == 2 ? "Solo" : strEmpty, 8, 18);
            else               lbl.SetText("Dup");
        }


        void UpdateMixerMuteAll(Label lbl)
        {
            if (ShowMixer > 0) lbl.SetText(ShowMixer == 2 ? "Mute" : strEmpty, 8, 18);
            else               lbl.SetText("Del");
        }
    }
}
