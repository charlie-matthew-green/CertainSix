using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace ShippingContainerSpoilage.WebApi
{
    public class OkResultWithWeakETag<T>: OkNegotiatedContentResult<T>
    {
        private readonly string eTag;

        public OkResultWithWeakETag(T content, string eTag, ApiController controller)
            : base(content, controller)
        {
            this.eTag = eTag;
        }

        public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await base.ExecuteAsync(cancellationToken);
            response.Headers.ETag = new EntityTagHeaderValue(eTag, true);
            return response;
        }        
    }
}