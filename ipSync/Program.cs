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
        config = JsonSerializer.Deserialize<Config>(File.ReadAllText(@"config.json")) ?? config;

        // 读取上次的IP
        LoadLastIP();

        logger.Info("程序已启动，正在监控IP地址变化...");

        // 定时调用函数功能
        do {
            CheckIP();
            Thread.Sleep(TimeSpan.FromSeconds(config.Interval));
        } while (true);
    }

    private static void CheckIP() {
        string currentIP = GetLocalIPAddress();
        if (currentIP != lastIP) {
            logger.Info("IP发生变化，当前IP：" + currentIP);
            lastIP = currentIP;
            SaveLastIP(currentIP);
            SyncIP(currentIP);
        } else {
            logger.Debug("IP未发生变化，当前IP：" + currentIP);
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

        return config.IsIPv6 ? ipv6Address : ipv4Address;
    }

    private static void SyncIP(string ip) {
        SendEmail(ip);
        SyncToServer(config.DeviceName, ip);
    }

    private static void SendEmail(string ip) {
        try {
            logger.Info("邮件同步IP");
            string body = $"{config.DeviceName}={ip}";
            string subject = $"{config.Email.TitlePrefix}" + body;

            // 创建MailMessage对象
            MailMessage mail = new MailMessage {
                From = new MailAddress(config.Email.Username),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mail.To.Add(config.Email.ToAddress);

            // 创建SmtpClient对象
            using (var client = new SmtpClient(config.Email.SmtpServer, config.Email.SmtpPort)) {
                client.EnableSsl = config.Email.EnableSsl;
                client.Credentials = new NetworkCredential(config.Email.Username, config.Email.Password);

                // 发送邮件
                client.Send(mail);
            }

            logger.Info("邮件同步成功!");
        } catch (Exception e) {
            logger.Info("邮件同步失败：" + e.Message);
        }

    }

    private static void SyncToServer(string name, string ip) {
        var client = new RestClient(config.IpSyncUrl);
        var request = new RestRequest();
        request.AddParameter("pswd", config.ApiPswd);
        request.AddParameter("name", name);
        request.AddParameter("ip", ip);

        var response = client.Execute(request);
        if (response.IsSuccessful) {
            logger.Info("接口同步IP成功!");
        } else {
            logger.Error("接口同步IP失败：" + response.ErrorMessage);
        }
    }
}
