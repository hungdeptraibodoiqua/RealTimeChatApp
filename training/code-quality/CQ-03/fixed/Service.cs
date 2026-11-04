// TÌNH HUỐNG ĐÀO TẠO, KHÔNG LÀ MÃ NGUỒN ỨNG DỤNG
using System.Threading;
using System.Threading.Tasks;
public class CQ03
{
    public async Task RunAsync(CancellationToken ct) { await Task.Delay(50, ct); await Task.Delay(1000, ct); }
}
