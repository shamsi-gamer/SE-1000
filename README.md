# SE-909

## Session

## Clips

## Instruments

## Parameters

## Samples

This is an additive wavetable synth. By combining up to 120 waves at different volumes a wide range of sounds can be generated. The wave samples are generated by [SE-909 Sounds](https://github.com/shamsi-gamer/SE-909-Sounds) and have to be modded in.

### Tones

* Sine
* Triangle
* Sawtooth
* Square

### Noise

The noise samples line up with the piano keys with equal frequency steps for the cutoff point, which makes them easy to line up with tone samples. Low and high noise are completely dry with no resonance.

* _Low noise_: white noise through a low-pass filter, with the lowest filtered frequencies in the lowest notes, and the full white noise in the highest notes.
* _High noise_: the opposite, with full white noise in the lowest frequencies and highest filtered frequencies in the highest notes.
* _Band noise_: a span of 5 notes worth of noise frequencies, ±2 around the played note. This helps to make metallic sounds or give interesting flavor to other sounds.
* _Click_: a high-to-low sine sweep, slow and pronounced in the lowest notes, lasery in the middle, and quick and clicky in the highest notes. In the very highest notes when it starts aliasing, the clicks become very dry, so there's quite a range.
* _Crunch_: initially added to make claps, as 60fps is not fast enough to play the individual clicks inside the clap. This is still in question, it may have to be reworked.

## IO
