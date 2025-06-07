using System;
using System.Collections.Generic;

namespace Client_Get_Token.Models;

public partial class GoogleToken
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string? AccessToken { get; set; }

    public string RefreshToken { get; set; } = null!;

    public string? TokenType { get; set; }

    public int? ExpiresIn { get; set; }

    public string? Scope { get; set; }

    public DateTime IssuedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
