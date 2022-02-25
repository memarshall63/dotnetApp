using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;

namespace dotnetApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;


	/* Create a complex Context class for carrying around values we always want to log */
	private class Context
	{	
		public class Innercontext 
		{
			public int SubCount { get; set; }
			public string SubSession { get; set; }
			public Innercontext(int subcount, string subsession)
			{
				SubCount = subcount;
				SubSession = subsession;
			}
		}

		public int Count { get; set; }
		public string Session { get; set; }
		public string User { get; set; }
		public Innercontext Subcontext;

		public Context(int count, string session, string user)
		{
			Count = count;
			Session = session;
			User = user;
			Subcontext = new Innercontext((count * 3), session + ":" + session + "|" + session);
		}
	}

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {

	    /* set some context values into our Context() object */
	    var _context = new Context(42,"SESS_ABCD_EFGH","marshall");

	    /* METHOD 1 - Create some log entries by serializing the Context object into the message*/
	    _logger.LogDebug("IEnumberable<WeatherForecast> Get().LogDebug {context}",JsonConvert.SerializeObject(_context));
	    _logger.LogTrace("IEnumberable<WeatherForecast> Get().LogTrace {context}",JsonConvert.SerializeObject(_context));
	    _logger.LogInformation("IEnumberable<WeatherForecast> Get().LogInformation {context}",JsonConvert.SerializeObject(_context));
	    _logger.LogWarning("IEnumberable<WeatherForecast> Get().LogWarning {context}",JsonConvert.SerializeObject(_context));
	    _logger.LogError("IEnumberable<WeatherForecast> Get().LogError {context}",JsonConvert.SerializeObject(_context));
	    _logger.LogCritical("IEnumberable<WeatherForecast> Get().LogCritical {context}",JsonConvert.SerializeObject(_context));

	    /* A second method is available via teh TelemetryClient() class.. but it takes a bit more to inject the structure */
	    /* Method 2 - Transform the Context object into a PropertyInfo array (reflection)*/
	    PropertyInfo[] infos = _context.GetType().GetProperties();

	    /* Method 2b - transform the PropertyInfo array into a Dictionary<string, string> */
	    Dictionary<string,string> dict = new Dictionary<string, string> ();
	    foreach (PropertyInfo info in infos)
	    {
		    dict.Add(info.Name, info.GetValue(_context, null).ToString());
	    }

	    /* Method 2c - use the TelemetryClient().TraceTrace to log a custom trace message */
	    var telemetryClient = new TelemetryClient();
	    /*
	     * You'll definitely want to use your own Instrumentation Key as required.
	     */
	    telemetryClient.InstrumentationKey="f1869a0c-5f27-40de-8847-220057fd08c4";
	    
	    /* Using a dictionary object */
	    telemetryClient.TrackTrace("TrackTrace Trace Message using Dictionary {context}",
			    Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information,
			    dict);

	    /* Of course, you could do something like this, as well... */
	    string strContext = String.Format("TrackTrace Trace Message using JSON Serialization {0}", JsonConvert.SerializeObject(_context));
	    telemetryClient.TrackTrace(strContext, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);

	    /* the TelemetryClient() also allows for posting custom events and custom metrics */
	    telemetryClient.TrackEvent("This is a TrackEvent custom event");
	    telemetryClient.TrackMetric("Customer_TrackMetric_Meaning_of_Life_Metric", 42);

	    /* back to the sample code */
            var rng = new Random();

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
