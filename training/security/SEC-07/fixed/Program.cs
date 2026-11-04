builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:3000")
  .AllowAnyHeader().AllowAnyMethod()));
