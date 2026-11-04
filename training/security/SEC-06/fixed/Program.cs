options.TokenValidationParameters = new TokenValidationParameters {
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};
