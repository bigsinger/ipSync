using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text.Json;
using NLog;
using RestSharp;

namespace ipSync;
internal class Program {
    private static Config config = new();
    private static string lastIP = string.Empty;
    public static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static void Main(string[] args) {
        LogManager.Setup().LoadConfigurationFromFile("NLog.config");

        // 读取配置文件
        config = JsonSerializer.Deserialize<Config>(File.ReadAllText(@"..\Data\config.json")) ?? config;

        // 读取上次的IP
        LoadLastIP();

        logger.Info("程序已启动，正在监控IP地址变化...");

        // 调用一次函数功能
        CheckIP();

        // 定时调用函数功能
        var timer = new Timer(TimerCallback, null, config.Interval * 1000, config.Interval * 1000);
    }

    private static void TimerCallback(object? state) {
        CheckIP();
    }

    private static void CheckIP() {
        string currentIP = GetLocalIPAddress();
        if (currentIP != lastIP) {
            lastIP = currentIP;
            SaveLastIP(currentIP);
            SyncIP(currentIP);
        }
    }

    // 读取上次的IP
    static void LoadLastIP() {
        if (File.Exists("lastIP.txt")) {
            lastIP = File.ReadAllText("lastIP.txt");
        } else {
            lastIP = string.Empty;
        }
    }

    static void SaveLastIP(string ip) {
        File.WriteAllText("lastIP.txt", ip);
    }

    private static string GetLocalIPAddress() {
        string ipv6Address = string.Empty;
        string ipv4Address = string.Empty;

        foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
            if (address.AddressFamily == AddressFamily.InterNetworkV6) {
                ipv6Address = address.ToString();
            } else if (address.AddressFamily == AddressFamily.InterNetwork) {
                ipv4Address = address.ToString();
            }
        }

        return ipv6Address ?? ipv4Address;
    }

    private static void SyncIP(string ip) {
        SendEmail(ip);
        SyncToServer(ip);
    }

    private static void SendEmail(string ip) {
        try {
            logger.Info("邮件同步IP");
            string text = $"[ip]:{config.DeviceName}={ip}";

            // 创建MailMessage对象
            MailMessage mail = new MailMessage {
                From = new MailAddress(config.Email.Username),
                Subject = text,
                Body = text
            };
            mail.To.Add(config.Email.ToAddress);

            // 创建SmtpClient对象
            using (SmtpClient smtpClient = new SmtpClient(config.Email.SmtpServer, config.Email.SmtpPort)) {
                smtpClient.EnableSsl = config.Email.EnableSsl;
                smtpClient.Credentials = new NetworkCredential(config.Email.Username, config.Email.Password);

                // 发送邮件
                smtpClient.Send(mail);
            }

            logger.Info("邮件同步成功!");
        } catch (Exception e) {
            logger.Info("邮件同步失败：" + e.Message);
        }

    }

    private static void SyncToServer(string ip) {
        var client = new RestClient(config.IpSyncUrl);
        var request = new RestRequest();
        request.AddParameter("name", "test");
        request.AddParameter("pswd", "your_password_here");
        request.AddParameter("ip", ip);

        var response = client.Execute(request);
        if (response.IsSuccessful) {
            logger.Info("接口同步IP成功!");
        } else {
            logger.Error("接口同步IP失败：" + response.ErrorMessage);
        }
    }
}
