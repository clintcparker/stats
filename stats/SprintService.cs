using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using System.Collections.Specialized;

namespace stats;
public class SprintService : ISprintService
{
    public const string ITERATIONS_PATH = "/_odata/v3.0-preview/Iterations";
    public const string TEAMS_PATH = "/_odata/v3.0-preview/Teams";
    public const string PROCESSES_PATH = "/_odata/v3.0-preview/Processes";
    public const string WORKITEMS_PATH = "/_odata/v4.0-preview/WorkItems";
    public const string WORKITEM_SNAPSHOT_PATH = "/_odata/v4.0-preview/WorkItemSnapshot";
    public const string PROCESS_CONFIG_PATH = "/_apis/work/processconfiguration?api-version=5.0-preview.1";
    public HttpClient analyticsClient;
    private readonly string OptionPAT;
    private readonly string OptionInstance;
    private readonly List<string> OptionTeams;
    private readonly string OptionProjectName;
    private readonly string WorkItemTypes = " (WorkItemType eq 'Bug' or WorkItemType eq 'Divider' or WorkItemType eq 'Internal' or WorkItemType eq 'Product Release' or WorkItemType eq 'Spike' or WorkItemType eq 'User Story')";

    public SprintService(SprintOptions options, HttpClient httpClient)
    {
        this.OptionProjectName = options.Project;
        this.OptionInstance = options.Instance;
        this.OptionTeams = options.Teams.ToList();
        this.OptionPAT = options.Pat;
        this.analyticsClient = httpClient;
        SetUpHttpClient(OptionPAT);
    }
    public SprintService()
    {
        this.OptionProjectName = "ClockShark";
        this.OptionInstance = "clockshark";
        this.OptionTeams = new List<string> { "Cuckoo" };
        this.analyticsClient = new ADOAnalyticsHttpClient(OptionPAT);
    }

    // public SprintService(object options)
    // {
    //     this.OptionProjectName = options.ProjectName;
    //     this.OptionInstance = options.Instance;
    //     this.OptionTeams = options.Teams;
    //     this.OptionPAT = options.PAT;
    //     this.analyticsClient = new ADOAnalyticsHttpClient(OptionPAT);
    // }

    public void SetUpHttpClient(string PAT)
    {
        
            analyticsClient.BaseAddress = new Uri("https://analytics.dev.azure.com/");
            analyticsClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", PAT))));
            analyticsClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            analyticsClient.DefaultRequestHeaders.Host = "analytics.dev.azure.com";
    }

    public async Task<List<TeamIteration>> getSprints(DateTime startDate, DateTime endDate)
    {
        var sprints = new List<TeamIteration>();
        var teams = await getTeamsForProject(OptionProjectName);
        teams = teams.Where(t => OptionTeams.Contains(t.TeamName)).ToList();
        foreach (var team in teams)
        {
            var teamSprints = await getIterationsForTeam(team, startDate, endDate);
            foreach (var sprint in teamSprints)
            {
                sprint.TeamName = team.TeamName;
                var workItemPoints = await getWorkItemPoints(team, sprint);
                var plannedPoints = await getPlannedWorkItemPoints(team, sprint);
                var latePoints = await getWorkItemsCompletedLate(team, sprint);
                var completedPoints = workItemPoints.Where(wi => wi.StateCategory == "Completed").Sum(wi => wi.StoryPoints);
                var incompletePoints = workItemPoints.Where(wi => wi.StateCategory == "InProgress").Sum(wi => wi.StoryPoints);
                sprint.CompletedPoints = completedPoints;
                sprint.PlannedPoints = plannedPoints.Sum(wi => wi.StoryPoints);
                sprint.LatePoints = latePoints.Sum(wi => wi.StoryPoints);
                sprint.IncompletePoints = incompletePoints;
            }
            sprints.AddRange(teamSprints);
        }
        return sprints;
    }

    public async Task<List<TeamIteration>> getSprints(int count)
    {
        var sprints = new List<TeamIteration>();
        var teams = await getTeamsForProject(OptionProjectName);
        teams = teams.Where(t => OptionTeams.Contains(t.TeamName)).ToList();
        foreach (var team in teams)
        {
            var teamSprints = await getIterationsForTeam(team, count);
            foreach (var sprint in teamSprints)
            {
                sprint.TeamName = team.TeamName;
                var workItemPoints = await getWorkItemPoints(team, sprint);
                var plannedPoints = await getPlannedWorkItemPoints(team, sprint);
                var latePoints = await getWorkItemsCompletedLate(team, sprint);
                var completedPoints = workItemPoints.Where(wi => wi.StateCategory == "Completed").Sum(wi => wi.StoryPoints);
                var incompletePoints = workItemPoints.Where(wi => wi.StateCategory == "InProgress").Sum(wi => wi.StoryPoints);
                sprint.CompletedPoints = completedPoints;
                sprint.PlannedPoints = plannedPoints.Sum(wi => wi.StoryPoints);
                sprint.LatePoints = latePoints.Sum(wi => wi.StoryPoints);
                sprint.IncompletePoints = incompletePoints;
            }
            sprints.AddRange(teamSprints);
        }
        return sprints;
    }

    public async Task<List<TeamIteration>> getIterationsForTeam(Team team, DateTime startDate, DateTime endDate)
    {
        NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

        //queryString.Add("$filter", $"Teams/any(t:t/TeamSK eq {team.TeamId}) and StartDate lt now()");
        queryString.Add("$filter", $"Teams/any(t:t/TeamSK eq {team.TeamSK}) and startDate ge {startDate.ToString("yyyy-MM-dd")} and endDate le {endDate.ToString("yyyy-MM-dd")}");
        queryString.Add("$orderby", "IsEnded,StartDate desc,EndDate desc,IterationName");
        queryString.Add("$select", "IterationSK,IterationName,StartDate,EndDate,IsEnded,IterationPath");
        queryString.Add("$top", "100");

        var filter = queryString.ToString(); // Returns "key1=value1&key2=value2", all URL-encoded
                                             //var filter = HttpUtility.UrlPathEncode($"teamSK eq '{team.TeamSK}' and startDate ge {startDate.ToString("yyyy-MM-dd")} and endDate le {endDate.ToString("yyyy-MM-dd")}");
        var path = $"{OptionInstance}/{OptionProjectName}{ITERATIONS_PATH}?{filter}";
        //var iterations = await analyticsClient.GetFromJsonAsync<IterationResponse>(path);
        var iterations = await analyticsClient.GetFromJsonAsync<ADOResponse<TeamIteration>>(path);
        return iterations.value;
    }

    public async Task<List<TeamIteration>> getIterationsForTeam(Team team, int count)
    {
        NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("$filter", $"Teams/any(t:t/TeamSK eq {team.TeamSK}) and StartDate lt now()");
        //queryString.Add("$filter", $"Teams/any(t:t/TeamSK eq {team.TeamSK}) and startDate ge {startDate.ToString("yyyy-MM-dd")} and endDate le {endDate.ToString("yyyy-MM-dd")}");
        queryString.Add("$orderby", "IsEnded,StartDate desc,EndDate desc,IterationName");
        queryString.Add("$select", "IterationSK,IterationName,StartDate,EndDate,IsEnded,IterationPath");
        queryString.Add("$top", count.ToString());

        var filter = queryString.ToString(); // Returns "key1=value1&key2=value2", all URL-encoded
                                             //var filter = HttpUtility.UrlPathEncode($"teamSK eq '{team.TeamSK}' and startDate ge {startDate.ToString("yyyy-MM-dd")} and endDate le {endDate.ToString("yyyy-MM-dd")}");
        var path = $"{OptionInstance}/{OptionProjectName}{ITERATIONS_PATH}?{filter}";
        //var iterations = await analyticsClient.GetFromJsonAsync<IterationResponse>(path);
        var iterations = await analyticsClient.GetFromJsonAsync<ADOResponse<TeamIteration>>(path);
        return iterations.value;
    }




    public async Task<List<Team>> getTeamsForProject(string projectId)
    {
        var path = $"{OptionInstance}/{projectId}{TEAMS_PATH}";
        //var teams = new List<Team>();
        var teams = await analyticsClient.GetFromJsonAsync<ADOResponse<Team>>(path);
        return teams.GetValue();
    }




    public async Task<List<WorkItemPoints>> getWorkItemPoints(Team team, TeamIteration iteration, string effortWord = "StoryPoints")
    {
        NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("$apply", $"filter(Teams/any(t:t/TeamSK eq {team.TeamSK}) and {WorkItemTypes} and (IterationSK eq {iteration.IterationSK}) and StateCategory ne null)/groupby((StateCategory,State,IterationSK),aggregate(StoryPoints with sum as StoryPoints))");
        var query = queryString.ToString(); // Returns "key1=value1&key2=value2", all URL-encoded
        var path = $"{OptionInstance}/{OptionProjectName}{WORKITEMS_PATH}?{query}";
        var workItems = await analyticsClient.GetFromJsonAsync<ADOResponse<WorkItemPoints>>(path);
        return workItems.value;

        /*
        and (WorkItemType eq 'Bug' or WorkItemType eq 'Divider' or WorkItemType eq 'Internal' or WorkItemType eq 'Product Release' or WorkItemType eq 'Spike' or WorkItemType eq 'User Story') 
        
        */
    }



    public async Task<List<WorkItemPoints>> getPlannedWorkItemPoints(Team team, TeamIteration iteration, int planDays = 3, string effortWord = "StoryPoints")
    {
        NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
        var iterationStart = iteration.StartDate.AddDays(planDays).ToString("yyyyMMdd");
        queryString.Add("$apply",
        @$"filter(
                Teams/any(t:t/TeamSK eq {team.TeamId}) 
                    and (
                            IterationSK eq {iteration.IterationSK} 
                        )
                    and {WorkItemTypes}
                    and (RevisedDateSK eq null or RevisedDateSK gt {iterationStart})
                    and StateCategory ne null
                    and DateSK eq {iterationStart}
                    )/groupby((IterationSK),aggregate(StoryPoints with sum as StoryPoints))"
        );
        var query = queryString.ToString(); // Returns "key1=value1&key2=value2", all URL-encoded
        var path = $"{OptionInstance}/{OptionProjectName}{WORKITEM_SNAPSHOT_PATH}?{query}";
        var workItems = await analyticsClient.GetFromJsonAsync<ADOResponse<WorkItemPoints>>(path);
        return workItems.value;
    }

    public async Task<List<WorkItemPoints>> getWorkItemsCompletedLate(Team team, TeamIteration iteration, int lateDays = 2, string effortWord = "StoryPoints")
    {        /*
                let endDate = velocityOpts.lateAfterDays >= 0 ? tfsUtils.buildDateStringForAnalytics(iteration.EndDate, velocityOpts.lateAfterDays) : `Iteration/EndDate`;
        let workItemLine = buildWorkItemLine(backlogTypes);
        let queryParameters = {
            "$apply":`filter(Teams/any(t:t/TeamSK eq ${teamId}) 
                    ${workItemLine}
                    and StateCategory eq 'Completed' 
                    and (IterationSK eq ${iteration.IterationSK}) 
                    and CompletedDate gt ${endDate}) 
                    /
                    groupby((StateCategory, IterationSK),aggregate(${velocityOpts.effortWord} with sum as AggregationResult))`
        };

        let path= `/${velocityOpts.projectId}${WORKITEMS_PATH}?${querystring.stringify(queryParameters)}`;
        let rawWorkItems = await tfsUtils.analyticsRequest(path);
        let workItems = rawWorkItems.value;
        let late = 0;
        if (workItems && workItems[0])
        {
            late = workItems[0].AggregationResult ? workItems[0].AggregationResult : 0;
        }
        return late;
        */


        NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
        var iterationEnd = iteration.EndDate.AddDays(lateDays).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");// 2023-06-22T06:59:59.999Z
        queryString.Add("$apply",
        @$"filter(Teams/any(t:t/TeamSK eq {team.TeamSK}) 
                    and {WorkItemTypes}
                    and StateCategory eq 'Completed' 
                    and (IterationSK eq {iteration.IterationSK}) 
                    and CompletedDate gt {iterationEnd}) 
                    /
                    groupby((StateCategory, IterationSK),aggregate({effortWord} with sum as AggregationResult))"
        );
        var query = queryString.ToString(); // Returns "key1=value1&key2=value2", all URL-encoded
        var path = $"{OptionInstance}/{OptionProjectName}{WORKITEM_SNAPSHOT_PATH}?{query}";
        var workItems = await analyticsClient.GetFromJsonAsync<ADOResponse<WorkItemPoints>>(path);
        return workItems.value;
    }

}

public class Team
{
    public string TeamName { get; set; }
    public string TeamSK { get; set; }
    public string TeamId { get; set; }
    public string ProjectSK { get; set; }
}


public class TeamIteration
{
    public TeamIteration()
    {
        WorkItems = new List<string>();
    }
    public string IterationName { get; set; }
    public string IterationSK { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsEnded { get; set; }

    public decimal? CompletedPoints { get; set; }

    public string TeamName { get; set; }

    public List<string> WorkItems { get; set; }

    public decimal? LatePoints { get; set; }

    public decimal? PlannedPoints { get; set; }
    public decimal? IncompletePoints { get; set; }

    public string StartDateSK { get; set; }
}

public class WorkItemPoints
{
    public string IterationSK { get; set; }
    public string StateCategory { get; set; }
    public decimal? StoryPoints { get; set; }
}