# Ảnh Docker của các công cụ quét, dùng chung cho run-lab.ps1, demo-15-cases.ps1, scan-local.ps1.
# BƯỚC 1c (bắt buộc trước khi chụp ảnh nộp bài): thay từng giá trị bằng dạng image@sha256:<digest>
#   docker pull <image:phien-ban>; docker image inspect <image:phien-ban> --format '{{index .RepoDigests 0}}'
# Hoặc đặt biến môi trường cùng tên (GITLEAKS_IMAGE, CHECKOV_IMAGE, OPA_IMAGE, TRIVY_IMAGE, PYTHON_IMAGE).
# TRIVY: KHÔNG dùng 0.69.4, 0.69.5, 0.69.6 (sự cố chuỗi cung ứng 03/2026, advisory GHSA-69fq-xp46-6x23).
function Get-ToolImage([string]$name, [string]$default) {
  $v = [Environment]::GetEnvironmentVariable($name)
  if ([string]::IsNullOrWhiteSpace($v)) { return $default } else { return $v }
}
$GITLEAKS_IMAGE = Get-ToolImage 'GITLEAKS_IMAGE' 'zricethezav/gitleaks@sha256:c00b6bd0aeb3071cbcb79009cb16a60dd9e0a7c60e2be9ab65d25e6bc8abbb7f'
$CHECKOV_IMAGE  = Get-ToolImage 'CHECKOV_IMAGE'  'bridgecrew/checkov@sha256:4e4f2051941d2891f2b1ed7d20eb5a188c80f980f4af8bbe3bfd6a33ce674304'
$OPA_IMAGE      = Get-ToolImage 'OPA_IMAGE'      'openpolicyagent/opa@sha256:895872de1e8dc5b82e04f9410d2d1ada2f72d28fdcdba0e8789d91cfc27ec0d0'
$TRIVY_IMAGE    = Get-ToolImage 'TRIVY_IMAGE'    'aquasec/trivy@sha256:790fd66d1697395d8693d1acaa1ea702406a20f109b6fc7bac8db623eca6c06c'
$PYTHON_IMAGE   = Get-ToolImage 'PYTHON_IMAGE'   'python@sha256:e6dae589c1c05976188fd3bd019afd091445fc533ed2ea29001beddc7368a9c8'

$unpinned = @($GITLEAKS_IMAGE, $CHECKOV_IMAGE, $OPA_IMAGE, $TRIVY_IMAGE, $PYTHON_IMAGE) | Where-Object { $_ -notmatch '@sha256:' }
if ($unpinned) {
  Write-Warning ("Chua ghim digest cho: " + ($unpinned -join ', ') + ". Ket qua chi dung de thu, KHONG dung lam minh chung nop bai (xem Buoc 1c).")
}
