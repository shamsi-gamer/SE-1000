namespace IngameScript
{
    partial class Program
    {
        void InitMixerLabels()
        { 
            lblMixerVolumeUp   = new Label(Lbl("M Up R"));
            lblMixerVolumeDown = new Label(Lbl("M Down R"));
            lblMixerAll        = new Label(Lbl("M Solo R"));
            lblMixerMuteAll    = new Label(Lbl("M Mute R"));

            lblMixerShift = new Label(
                Lbl("M Shift"), 
                lbl => CurClip.MixerShift);

            lblSession = new Label(
                Lbl("Session"), 
                lbl => g_showSession && g_setClip,
                null,
                lbl => lbl.SetText(g_showSession ? "Clip" : "Clips", 8, 18));
        }
    }
}
