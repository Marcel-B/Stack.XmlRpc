using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using com.b_velop.XmlRpc.BL;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Contexts;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Code
{
    public class ParserImpl : Parser
    {
        private readonly IMemoryCache _cache;
        private readonly HomeContext _db;
        private readonly AlarmService _alarmService;
        private readonly ILogger<Parser> _logger;
        private readonly IDictionary<string, bool> _alarmIds;
        private readonly ActiveMeasurePointService _activeMeasurePointService;

        public ParserImpl(
            IMemoryCache cache,
            HomeContext homeContext,
            AlarmService alarmService,
            ActiveMeasurePointService activeMeasurePointService,
            ILogger<Parser> logger)
        {
            _cache = cache;
            _db = homeContext;
            _alarmService = alarmService;
            _activeMeasurePointService = activeMeasurePointService;
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

                if (!_cache.TryGetValue(Strings.ActiveMeasurePoints, out string[] activeIds))
                {
                    _logger.LogWarning($"No ActiveMeasurePoint available, fetch activeMeasurePoints");
                    // Load activeMeasurePoins
                    var activeMeasurePoints = await _activeMeasurePointService.GetActiveMeasurePointsAsync();

                    if (activeMeasurePoints == null)
                        return;

                    activeIds = activeMeasurePoints.Where(_ => _.IsActive).Select(_ => _.ExternId).ToArray();
                    _cache.Set(Strings.ActiveMeasurePoints, activeIds.ToArray());
                    _cache.Set(Strings.LastActiveMeasurePointsPull, DateTime.Now);
                }

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

                            // Persist state to db as backup
                            var value = await _db.States.FindAsync(1);
                            _db.States.Update(value);
                            value.Updated = DateTime.Now;
                            value.State = state;
                            _ = await _db.SaveChangesAsync();

                            _cache.Set(Strings.AlarmActive, state);
                            _logger.LogInformation($"Alarm '{homematicValue.AllId}' push to {_alarmIds[homematicValue.AllId]}");
                            await _alarmService.UpdateAlarmAsync();
                        }

                        if (activeIds.Contains(homematicValue.AllId))
                        {
                            var doublette = values.FirstOrDefault(_ => _.AllId == homematicValue.AllId && _.Time == homematicValue.Time);
                            if (doublette != null)
                            {
                                _logger.LogInformation($"False Value: Contains '{doublette}'\n'{homematicValue}'");
                            }
                            values.Add(homematicValue);
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