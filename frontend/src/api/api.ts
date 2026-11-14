import { client, getApiCompetitionsOverview, getApiCompetitionsTeamOverview, getApiLandingGet } from "./client";

class ApiClient {
  constructor() {
    client.setConfig({
      // set default base url for requests
      baseUrl: import.meta.env.VITE_API_URL,
      // set default headers for requests
      headers: {
        //Authorization: "Bearer <token_from_service_client>",
      },
    });
  }

  getCompetitionOverview(cupId: string, date?: string) {
    return getApiCompetitionsOverview({ query: { cupId, date } });
  }

  getTeamCompetitionOverview(cupId: string, date?: string) {
    return getApiCompetitionsTeamOverview({ query: { cupId, date } });
  }

  getLandingData() {
    return getApiLandingGet();
  }
}

export default new ApiClient();
