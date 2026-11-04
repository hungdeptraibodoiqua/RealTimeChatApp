// TÌNH HUỐNG ĐÀO TẠO, KHÔNG LÀ MÃ NGUỒN ỨNG DỤNG
public class Worker { private readonly ILogger<Worker> logger; public Worker(ILogger<Worker> logger) => this.logger = logger; public void Run() { try { int.Parse("x"); } catch (FormatException ex) { logger.LogWarning(ex, "Invalid input"); throw; } } }
