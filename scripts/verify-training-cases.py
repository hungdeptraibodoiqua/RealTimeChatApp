#!/usr/bin/env python3
"""Educational regression guard: 10 vulnerable examples MUST fail, 10 fixes MUST pass.
This is a pattern-based lab assertion, NOT a substitute for code review or security testing.
"""
from pathlib import Path
import json,re,sys,argparse
ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT/'training'/'security'
REPORT=ROOT/'security-reports'
FILES={
  'SEC-01':'appsettings.json', 'SEC-02':'appsettings.json',
  'SEC-03':'ChatHub.cs','SEC-04':'ChatHub.cs','SEC-05':'Program.cs',
  'SEC-06':'Program.cs','SEC-07':'Program.cs','SEC-08':'Dockerfile',
  'SEC-09':'compose.yaml','SEC-10':'compose.yaml'
}
def issue(code,txt):
    if code=='SEC-01': return bool(re.search(r'"Secret"\s*:\s*"([^"\r\n]+)"',txt))
    if code=='SEC-02': return bool(re.search(r'Password=[^;"\s]+',txt))
    if code in ('SEC-03','SEC-04'):
        return not ('GetMemberAsync' in txt and 'LeftAtUtc' in txt and 'HubException' in txt)
    if code=='SEC-05': return bool(re.search(r'\bapp\.UseStaticFiles\s*\(',txt))
    if code=='SEC-06': return bool(re.search(r'ValidateLifetime\s*=\s*false',txt))
    if code=='SEC-07': return bool(re.search(r'\.AllowAnyOrigin\s*\(',txt))
    if code=='SEC-08':
        users=re.findall(r'^\s*USER\s+(\S+)',txt,re.M|re.I)
        return not users or users[-1].lower() in ('root','0')
    if code=='SEC-09': return bool(re.search(r'\bprivileged\s*:\s*true',txt,re.I))
    if code=='SEC-10': return bool(re.search(r'0\.0\.0\.0:5432:5432',txt))
    raise ValueError(code)
def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--out',default=str(REPORT/'training-validation.json'))
    args=parser.parse_args()
    results=[]
    good=True
    for cid,filename in FILES.items():
        for version in ('vulnerable','fixed'):
            p=BASE/cid/version/filename
            exists=p.is_file()
            finding=issue(cid,p.read_text(encoding='utf-8')) if exists else None
            expected=version=='vulnerable'
            passed=exists and finding==expected
            results.append({'case':cid,'version':version,'detected':finding,'expected':expected,'correct':passed,'path':str(p.relative_to(ROOT))})
            good &= passed
            print(('PASS' if passed else 'FAIL'),cid,version,'finding=',finding,'expected=',expected)
    Path(args.out).parent.mkdir(parents=True,exist_ok=True)
    Path(args.out).write_text(json.dumps({'all_expected':good,'results':results},ensure_ascii=False,indent=2),encoding='utf8')
    return 0 if good else 1
if __name__=='__main__': sys.exit(main())
