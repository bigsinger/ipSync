namespace ipSync;
public class Config {
    public string DeviceName { get; set; } = string.Empty;
    public int Interval { get; set; }
    public string IpSyncUrl { get; set; } = string.Empty;
    public EmailConfig Email { get; set; } = new EmailConfig();
}

public class EmailConfig {
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }   // SMTP服务器端口，通常为587或465
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}
