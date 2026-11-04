# (a) Code that -> ky vong exit 0
docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $GITLEAKS_IMAGE `
  dir /work --config /work/.gitleaks.toml --redact --exit-code 1
"Gitleaks working tree exit=$LASTEXITCODE"

# (b) Fixture SEC-01/02 -> vulnerable exit 1, fixed exit 0
foreach ($id in 'SEC-01','SEC-02') { foreach ($m in 'vulnerable','fixed') {
  docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $GITLEAKS_IMAGE `
    dir "/work/training/security/$id/$m" --config /work/training/gitleaks-demo.toml --redact --exit-code 1
  "$id $m exit=$LASTEXITCODE" } }
