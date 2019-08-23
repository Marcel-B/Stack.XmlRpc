using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using com.b_velop.XmlRpc.BL;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Code
{
    public class ParserImpl : Parser
    {
        private readonly IMemoryCache _cache;
        private readonly AlarmService _alarmService;
        private readonly ILogger<Parser> _logger;
        private readonly IDictionary<string, bool> _alarmIds;

        public ParserImpl(
            IMemoryCache cache,
            AlarmService alarmService,
            ILogger<Parser> logger)
        {
            _cache = cache;
            _alarmService = alarmService;
            _logger = logger;
            _cache.TryGetValue(Strings.AlarmIds, out _alarmIds);
        }

        public async Task Parse(
            Stream inputString)
        {
            try
            {
                if (!_cache.TryGetValue(Strings.Values, out HomematicValueList values))
                {
                    _logger.LogWarning($"Cache has no HomematicValueList. Create empty list.");
                    values = new HomematicValueList();
                }

                _cache.TryGetValue(Strings.ActiveMeasurePoints, out string[] activeIds);

                var doc = new XmlDocument();
                doc.Load(inputString);

                var nodeList = doc.GetElementsByTagName("value");

                if (nodeList == null)
                    return;

                for (var i = 0; i < nodeList.Count; i++)
                {
                    var node = nodeList[i]; // as XmlNode;
                    if (node?.InnerXml == Strings.InstanceId)
                    {
                        var homematicValue = new HomematicValue
                        {
                            Time = DateTimeOffset.Now,
                            Instance = node.InnerXml,
                            Id = nodeList[++i].InnerXml,
                            Name = nodeList[++i].InnerXml,
                            Value = nodeList[++i].InnerText,
                            Type = nodeList[i].FirstChild.Name,
                        };

                        if (_alarmIds.ContainsKey(homematicValue.AllId))
                        {
                            // Alarm knopf wurde gedrückt
                            var state = _alarmIds[homematicValue.AllId];
                            _logger.LogInformation($"Alarm '{homematicValue.AllId}' push to {_alarmIds[homematicValue.AllId]}");
                            await _alarmService.UpdateAlarmAsync();
                            _cache.Set(Strings.AlarmActive, state);
                        }
                        if (activeIds.Contains(homematicValue.AllId))
                        {
                            values.Add(homematicValue);
                            _logger.LogInformation($"{homematicValue} added to list");
                        }
                    }
                }
                _cache.Set(Strings.Values, values);
                _logger.LogInformation($"{values.Count} items in list.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while parsing");
            }
        }
    }
}