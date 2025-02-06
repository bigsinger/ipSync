namespace ipSync;
public class Config {
    public string DeviceName { get; set; } = string.Empty;
    public bool IsIPv6 { get; set; } = true;
    public int Interval { get; set; }
    public string IpSyncUrl { get; set; } = string.Empty;
    public string ApiPswd { get; set; } = string.Empty;
    public EmailConfig Email { get; set; } = new EmailConfig();
}

public class EmailConfig {
    public string TitlePrefix { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }   // SMTP服务器端口，通常为587或465
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;    // 授权码只显示1次，有效期180天
    public string ToAddress { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}
