﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Akavache;
using HuePod.Nasa;
using Refit;
using System.Reactive.Linq;
using Plugin.Connectivity;

namespace HuePod
{
	public class Service
	{
		INasaApi _api;

		public Service()
		{
			var refitSettings = new RefitSettings
			{
				UrlParameterFormatter = new ApodDateFormatter()
			};
			var client = new HttpClient()
			{
				BaseAddress = new Uri(Constants.NasaEndpoint)
			};

			BlobCache.ApplicationName = Constants.AkavacheAppName;

			_api = RestService.For<INasaApi>(client, refitSettings);
		}

		public async Task<bool> IsNasaAvailable()
		{
			return await CrossConnectivity.Current.IsReachable(Constants.NasaEndpoint);
		}

		public async Task<Apod> GetAstronomicPictureOf(DateTime? theDay = null)
		{
			var date = theDay.GetValueOrDefault(DateTime.Now);
			Func<Task<Apod>> func = async () =>
			{
			    var ap = await _api.GetAstronomicPictureOfSomeDay(date);
			    ap.CloudinaryUrl = "https://res.cloudinary.com/appod/image/fetch/c_limit,h_400,w_800/" + ap.Url;
                return ap;
			};

			var apod = await BlobCache.LocalMachine.GetOrFetchObject("pic" + $"{date:yyyyMMdd}",func);

			return apod;
		}

		public async Task<List<Apod>> GetLastAstronomicPictures(int days = 7)
		{
			var apod = await GetAstronomicPictureOf();
			var list = new List<Apod>(days);
			list.Add(apod);
			for (var i = 1; i < days; i++)
			{
				list.Add(await GetAstronomicPictureOf(apod.Date.AddDays(-1 * i)));
			}
			return list;
		}

	    public async Task ClearAll()
	    {
	        await BlobCache.LocalMachine.InvalidateAll();
	        await BlobCache.LocalMachine.Vacuum();

	    }
	}

	internal class ApodDateFormatter : IUrlParameterFormatter
	{
		#region IUrlParameterFormatter implementation

		public string Format(object value, ParameterInfo parameterInfo)
		{
			if (parameterInfo.ParameterType == typeof(DateTime))
				return ((DateTime)value).ToString("yyyy-MM-dd");
			return value.ToString();
		}

		#endregion
	}
}

