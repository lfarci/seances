# Azure hosting options

Status: 2026-10-16. The user confirmed a €5/month budget, Blazor SSR, and acceptable cold starts, and selected Container Apps conditional on it being cheaper for the workload. Final configuration and cost estimate remain to settle. No resources have been provisioned.

## Runtime distinction

GitHub Pages serves files, not an ASP.NET Core process. The earlier console collector ran on GitHub Actions, so no Azure compute was required to perform collection. A hosted .NET app is nevertheless a valid alternative if it simplifies the preferred development model.

## Candidate architecture

Recommend evaluating one Blazor Web App using static SSR and one container image:

```text
Browser -> Container App (Blazor SSR) -> Blob Storage JSON snapshot

Scheduled Container Apps Job (same image, refresh command)
  -> UGC public pages and fragments
  -> parse and validate
  -> replace Blob snapshot after success
```

- Server-rendered Razor components render the timetable and simplified list; use normal GET/query parameters for filters and a little browser JavaScript for saved settings and interaction where needed.
- Keep collection and rendering in the same repository/project. A command argument selects web-serving or one-off refresh mode; this is a packaging choice, not a separate API or frontend application.
- The web process reads the snapshot with a short in-memory cache; visitor requests do not trigger UGC scraping.
- Blob Storage supplies persistence across container restarts and scale-to-zero. There is no need for a relational database initially.
- A scheduled job runs independently of website traffic, using the same collection code. Do not rely on a timer inside a web app that scales to zero.
- Configure managed identities so the web app reads and the job writes the snapshot.
- Preserve the previous snapshot on collection failure. Publish one complete validated blob, and prevent overlapping refreshes with a lease if needed.

As of 2026-10-17, use Container Apps for the website and a scheduled Job sharing its image, with a JSON snapshot in Blob Storage. This minimizes packaging and deployment approaches within the chosen Blazor SSR design; it is not a claim that Jobs are universally cheaper than Functions. Deployment remains conditional on validating the cost estimate and detailed resource configuration.

## Comparison

| Choice | Benefit | Constraint |
| --- | --- | --- |
| GitHub Pages + Actions collector | Static serving and scheduled compute without Azure resources | Requires a static UI approach |
| App Service Free | Can host the .NET web app without a paid compute plan | 60 CPU minutes/day and no Always On; separate reliable scheduling needed |
| App Service Basic | A predictable baseline compute price and Always On | Runs a paid instance continuously; exact regional quote required |
| Container Apps Consumption + scheduled Job | Runs ordinary ASP.NET Core, scales idle web compute to zero, shares one image with collection | Variable charges and cold starts; scaling limits are not monetary caps |

## Cost guardrails to evaluate

- Consumption workload profile only, web minimum zero and maximum one replica, single active revision, and the smallest measured adequate CPU/memory allocation.
- Scheduled job with one replica, bounded execution timeout/retries, and protection against overlapping executions. Parallelism one alone does not prevent separate executions overlapping.
- A small Blob snapshot with bounded retention. Container-local storage is not durable storage.
- Avoid adding a paid registry solely for this app when a suitable public registry is available.
- Deliberately choose logging volume/retention and avoid logging every ordinary request or complete source responses. Logging, storage, and networking must be included in the estimate.
- No optional dedicated networking or private endpoints unless a concrete need appears.
- Budget alerts and rate limits help, but Azure budgets do not stop consumption. Even one continuously busy replica can cost more than the desired monthly target.
- Show both expected low-traffic cost and a continuously-active scenario before choosing resources. Do not promise a euro ceiling from free allowances or maximum replica count.

Container Apps Consumption grants currently include 180,000 vCPU-seconds, 360,000 GiB-seconds, and two million HTTP requests per subscription/month. A replica at zero incurs no app resource consumption charge. Job compute consumes applicable grants too; other resources have their own meters. Existing Static Web Apps bills do not determine this app's cost.

If a strict spending ceiling is essential, choose a hosting offer with an enforced quota/fixed resource allocation or explicitly accept availability limits. Azure budget alerts are not such a ceiling; automated responses also have detection delay.

## Decisions remaining

- €5/month is the confirmed budget target. A hard enforceable monetary cap is not provided by the proposed Azure consumption resources.
- Verify that the selected Container Apps configuration fits the budget and is cheaper than paid App Service for expected personal usage.
- Exact region, resource sizing, logging, and scheduled collection policy, followed by a concrete estimate.

## Regional reference estimate

Retrieved 2026-10-16 from the public Azure Retail Prices API, West Europe, EUR. These are budgeting references, not an account-specific quote; taxes and ancillary services are excluded.

| Meter | Rate |
| --- | ---: |
| Container Apps active CPU | €0.000030 / vCPU-second |
| Container Apps active memory | €0.000004 / GiB-second |
| Container Apps requests | €0.492784 / million after applicable grant |
| App Service Linux B1 | €0.015839 / hour |

At 0.25 vCPU and 0.5 GiB, the fully available monthly Container Apps grants cover approximately 200 active replica-hours shared between web app and job. For illustration, one active web hour/day plus ten collection minutes/day is 35 hours in a 30-day month, within those compute grants. This is a usage assumption, not measured demand or a complete bill estimate.

For 30 days continuously active, the same replica uses €17.78 of billable compute after entirely unused grants, or €24.62 if grants are already consumed. Requests, job compute, Blob operations/capacity, logs, registry, network transfer, and taxes are additional. Actual scale-down delay contributes to replica-hours and needs to be measured.

Linux App Service B1 is approximately €11.40 for 30 days of baseline compute at the retrieved rate, already above the €5 target. App Service Free remains a quota-limited alternative; Container Apps is the preferred candidate only if cold starts and variable consumption are acceptable.

Reproducible API queries (preview version retained because the older API rounded tiny EUR rates to zero):

- [Container Apps Standard meters](https://prices.azure.com/api/retail/prices?api-version=2023-01-01-preview&currencyCode=%27EUR%27&%24filter=serviceName%20eq%20%27Azure%20Container%20Apps%27%20and%20armRegionName%20eq%20%27westeurope%27%20and%20skuName%20eq%20%27Standard%27)
- [App Service B1 meters](https://prices.azure.com/api/retail/prices?api-version=2023-01-01-preview&currencyCode=%27EUR%27&%24filter=serviceName%20eq%20%27Azure%20App%20Service%27%20and%20armRegionName%20eq%20%27westeurope%27%20and%20skuName%20eq%20%27B1%27%20and%20priceType%20eq%20%27Consumption%27) — select product `Azure App Service Basic Plan - Linux`, meter `B1`.
- [Retail Prices API documentation](https://learn.microsoft.com/en-us/rest/api/cost-management/retail-prices/azure-retail-prices)

## Official sources

- [Container Apps billing and free grants](https://learn.microsoft.com/en-us/azure/container-apps/billing)
- [Container Apps jobs](https://learn.microsoft.com/en-us/azure/container-apps/jobs)
- [App Service quotas](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/azure-subscription-service-limits#app-service-limits)
- [Azure budgets](https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/tutorial-acm-create-budgets)
