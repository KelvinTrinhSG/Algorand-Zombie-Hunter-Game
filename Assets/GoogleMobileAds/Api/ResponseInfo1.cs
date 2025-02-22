// Copyright (C) 2020 Google, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using GoogleMobileAds.Common;
public class ResponseInfo1
{
    private IResponseInfoClient client;

    public ResponseInfo1(IResponseInfoClient client)
    {
        this.client = client;
    }

    public string GetMediationAdapterClassName()
    {
        return client.GetMediationAdapterClassName();
    }

    public string GetResponseId()
    {
        return client.GetResponseId();
    }

    public override string ToString()
    {
        return client.ToString();
    }
}
