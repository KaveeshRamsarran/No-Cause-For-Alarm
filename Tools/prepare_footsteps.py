"""Cut the user-supplied walking recording into contact-synchronised variations.
The owner confirmed free use; source files remain local, samples ship in the game.
"""
import pathlib,sys,json,hashlib
import numpy as np
import soundfile as sf
from scipy.signal import find_peaks
root=pathlib.Path(__file__).resolve().parents[1]
source=pathlib.Path(sys.argv[1]) if len(sys.argv)>1 else pathlib.Path.home()/'Downloads/ABDM/Music/Walk On Concrete - Sound Effect for editing.mp3'
audio,rate=sf.read(source)
if audio.ndim>1:audio=audio.mean(axis=1)
window=int(rate*.012)
envelope=np.convolve(np.abs(audio),np.ones(window)/window,mode='same')
peaks,_=find_peaks(envelope,distance=int(rate*.30),prominence=max(envelope)*.17)
out=root/'Assets/Resources/Audio/UserFootsteps';out.mkdir(parents=True,exist_ok=True)
count=0
for peak in peaks[:10]:
 start=max(0,peak-int(rate*.055));end=min(len(audio),peak+int(rate*.22))
 sample=audio[start:end].copy();sample-=sample.mean()
 fade=min(300,len(sample)//4);sample[:fade]*=np.linspace(0,1,fade);sample[-fade:]*=np.linspace(1,0,fade)
 sample*=.78/max(.001,float(np.max(np.abs(sample))))
 sf.write(out/('concrete%02d.wav'%count),sample,rate,subtype='PCM_16');count+=1
(root/'Artifacts/overhaul').mkdir(parents=True,exist_ok=True)
(root/'Artifacts/overhaul/footsteps.json').write_text(json.dumps({'source':source.name,'sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'sample_rate':rate,'contacts':count,'license':'User-provided; owner confirmed free use on 2026-09-17'},indent=2),encoding='utf8')
print('Prepared',count,'individual footstep contacts from supplied MP3')
