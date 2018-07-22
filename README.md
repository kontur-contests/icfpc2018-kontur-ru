# icfpc2018-kontur-ru
kontur.ru team @ ICFPC 2018

## Setup
1. Install Visual Studio 2017 version 15.7.4
1. Install .NET Core SDK 2.1.300 (includes .NET Core 2.1 Runtime) from https://github.com/dotnet/core/blob/master/release-notes/download-archives/2.1.0-download.md
1. Install the latest ReSharper / Rider version 2018.1.3

## URLs

1. Kibana (logs only): https://elk-test.skbkontur.ru/app/kibana#/discover?_g=()&_a=(columns:!(Message),index:'6fc7b9b0-8661-11e8-9e00-0f907f72b941',interval:auto,query:(language:lucene,query:''),sort:!('@timestamp',desc))
1. Elastic HTTP API (production data): http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io/ (NOTE: port 80)
1. Kibana (production data): http://efk2-kibana.efk2.10.217.14.7.xip.io/
1. Houston: https://wst.dev.kontur/hmon/index#/instances/pageDaemons (search for icfpc)
1. TeamCity: https://tc.skbkontur.ru/project.html?projectId=Icfpc18&tab=projectOverview
1. Octopus: https://octo.skbkontur.ru/app#/projects/icfpc18/overview
1. Отчёт с [самыми частыми экспешнами в Хьюстоне](http://efk2-kibana.efk2.10.217.14.7.xip.io/app/kibana#/visualize/edit/8af3ee80-8dd6-11e8-a15c-8df4033ed8a0?_g=(refreshInterval%3A(display%3AOff%2Cpause%3A!f%2Cvalue%3A0)%2Ctime%3A(from%3Anow-4h%2Cmode%3Aquick%2Cto%3Anow)))
