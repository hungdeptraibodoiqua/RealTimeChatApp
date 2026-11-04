#!/usr/bin/env python3
"""5 extra quality regression checks using deliberately simplified patterns."""
from pathlib import Path
import re,sys,json
R=Path(__file__).resolve().parents[1]
checks={
 'CQ-01':lambda t: bool(re.search(r'catch\s*\([^)]*\)\s*\{\s*\}',t)),
 'CQ-02':lambda t: bool(re.search(r'connected\s*&&\s*ready\s*&&\s*connected',t)),
 'CQ-03':lambda t: 'Thread.Sleep(' in t,
 'CQ-04':lambda t: 'return 937;' in t,
 'CQ-05':lambda t: len(re.findall(r'return\s+w\s*\*\s*h',t))>=2,
}
report=[]; ok=True
for k,fn in checks.items():
 for mode in ('vulnerable','fixed'):
  p=next((R/'training'/'code-quality'/k/mode).glob('*.cs'))
  detected=fn(p.read_text(encoding='utf8')); expected=mode=='vulnerable'; good=detected==expected
  print('PASS' if good else 'FAIL',k,mode,detected)
  ok&=good; report.append({'case':k,'mode':mode,'finding':detected,'expected':expected,'correct':good})
(R/'security-reports').mkdir(exist_ok=True)
(R/'security-reports'/'quality-validation.json').write_text(json.dumps(report,indent=2),encoding='utf8')
sys.exit(0 if ok else 1)
