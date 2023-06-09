﻿using Huawei.RabbitMqSubscriberService.Models;
using Huawei.RabbitMqSubscriberService.Services;
using Huawei.RabbitMqSubscriberService.ValidationModels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Huawei.RabbitMqSubscriberService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        private readonly RabbitMQClientService _rabbitMQClientService;

        private readonly IServiceProvider _serviceProvider;

        private Timer _timer;

        private IModel _channel;
        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogWarning($"Mail Doğrulama Arka Plan Hizmeti {DateTimeOffset.Now} de başlatıldı");
                _channel = _rabbitMQClientService.Connect();
                _channel.BasicQos(0, 0, false);
                return base.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("StartyAsync Hatası:{0}", ex.ToString());
                throw;
            }            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var consumer = new AsyncEventingBasicConsumer(_channel);
                _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);
                consumer.Received += Consumer_Received;
                _logger.LogWarning("Bilgi alma tamamlandı");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError("ExecuteAsync Hatası:{0}", ex.ToString());
                throw;
            }

        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                _logger.LogInformation("RabbitMq kuyruğundan mesaj alınıyor...");
                await Task.Delay(2000);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var body = @event.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var context = scope.ServiceProvider.GetRequiredService<MailValidationContext>();
                    Validation ventity = new Validation();
                    string errorDescription = "";

                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                        };
                        ventity = await System.Text.Json.JsonSerializer.DeserializeAsync<Validation>(stream, options).ConfigureAwait(false);
                    };

                    var vresult = await GetValidationResult(ventity.mailAddress);
                    if (vresult.errors != null)
                    {
                        if (vresult.errors.smtp != null)
                        {
                            errorDescription = vresult.smtp_debug[0].connection == false ? vresult.smtp_debug[0].errors.mailfrom : vresult.smtp_debug[0].errors.rcptto;
                        }
                        if (vresult.errors.regex != null)
                        {
                            errorDescription = vresult.errors.regex;
                        }
                    }
                    var success = vresult.success;
                    Validation v = new();
                    v.userID = ventity.userID;
                    v.BatchId = ventity.BatchId;
                    v.mailAddress = ventity.mailAddress;
                    v.Result = (bool)success ? "Başarılı" : "Başarısız";
                    v.ResultDescription = errorDescription;
                    v.RequestDate = ventity.RequestDate;
                    v.ResultDate = DateTime.Now.ToUniversalTime();
                    _logger.LogInformation("Doğrulanan mail adresi:{0} - Sonuç:{1} - Açıklama:{2}", v.mailAddress, v.Result, v.ResultDescription);
                    await AddValidation(v, context);
                }
                _logger.LogInformation("RabbitMq kuyruğundan mesaj alımı tamamlanıyor...");
                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Consumer_Received Hatası:{0}", ex.ToString());
                throw;
            }
        }

        private async Task<Root> GetValidationResult(string mailAddress)
        {
            Root response = new Root();
            try
            {
                var clientHandler = new HttpClientHandler();
                using (var http = new HttpClient(clientHandler))
                {
                    var endpoint = _configuration.GetValue<string>("TrueMail:ServiceUrl") + "?email=" + mailAddress;
                    http.DefaultRequestHeaders.Add("Authorization", _configuration.GetValue<string>("TrueMail:ValidationToken"));
                    var result = http.GetAsync(endpoint).Result;
                    var json = result.Content.ReadAsStringAsync().Result;
                    response = JsonConvert.DeserializeObject<Root>(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

            }
            return response;
        }

        public async Task AddValidation(Validation validation, MailValidationContext context)
        {
            await context.AddAsync(validation);
            await context.SaveChangesAsync();
            _logger.LogInformation("Sonuç veritabanına başarıyla kaydedildi!");

        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogWarning($"Mail Doğrulama Arka Plan Hizmeti {DateTimeOffset.Now} de durduruldu");

            return Task.CompletedTask;
        }

    }
}
