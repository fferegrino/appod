using System;
using System.Threading.Tasks;
using Refit;

namespace HuePod.Nasa
{
	[Headers("Accept: application/json")]
	public interface INasaApi
	{
		[Get("/planetary/apod?hd=true")]
		Task<Apod> GetAstronomicPictureOfSomeDay(DateTime date, string api_key = Constants.ApiKey);


	}
}

