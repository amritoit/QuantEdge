using AutoGen;
using AutoGen.Core;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace QuantEdge.Server.Services
{ 

    // Data transfer objects for agent tasks
    public class ResponseRefinementTask
    {
        public string OriginalResponse { get; set; } = string.Empty;
        public string UserQuery { get; set; } = string.Empty;
        public string RequiredTone { get; set; } = string.Empty;
    }

    public class ResponseQualityTask
    {
        public string Response { get; set; } = string.Empty;
        public string UserQuery { get; set; } = string.Empty;
        public List<string> CriteriaChecklist { get; set; } = new();
    }
}