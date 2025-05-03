using VotingBlockchain;

namespace BlockchainVoteUnits
{
    [TestClass]
    public sealed class BlockchainTests
    {
        [TestMethod]
        public async Task Test_That_Server_Responds_Correctly()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:5000/test");
            var content = await response.Content.ReadAsStringAsync();

            Assert.AreEqual("\"OK\"", content);
        }

        [TestMethod]
        public async Task Test_User_Exists()
        {
            string username = "000000000";
            var result = await Client.UserExists(username);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Test_Login_User_Is_Not_Null()
        {
            string username = "000000000";
            string password = "superSigma";
            var result = await Client.Login(username, password);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Test_Login_User_Is_000000000()
        {
            string username = "000000000";
            string password = "superSigma";
            var result = await Client.Login(username, password);

            Assert.AreEqual(username, result!.Username);
        }

        [TestMethod]
        public async Task Test_Get_Election_Is_Not_Null()
        {
            int id = 1;
            var result = await Client.GetElection(id);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Test_Get_Election_Is_Id_1()
        {
            int id = 1;
            var result = await Client.GetElection(id);

            Assert.AreEqual(id, result!.Id);
        }

        [TestMethod]
        public async Task Test_Get_Elections_Is_Not_Null()
        {
            var result = await Client.GetElections();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Test_Get_Current_Elections_Is_Not_Null()
        {
            var result = await Client.GetCurrentElections(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Test_Get_Current_Elections_Is_Current()
        {
            long utcNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var result = await Client.GetCurrentElections(utcNow);

            bool bRes = true;
            foreach (var i in result) 
            {
                if (!(i.StartDate < utcNow && utcNow < i.EndDate)) 
                { 
                    bRes = false;
                    break;
                }
            }

            Assert.IsTrue(bRes);
        }

        [TestMethod]
        public async Task Test_Get_Completed_Elections_Is_Not_Null()
        {
            var result = await Client.GetCompletedElections(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Test_Get_Completed_Elections_Is_Completed()
        {
            long utcNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var result = await Client.GetCompletedElections(utcNow);

            bool bRes = false;
            foreach (var i in result)
            {
                if (utcNow > i.EndDate)
                {
                    bRes = true;
                    break;
                }
            }

            Assert.IsTrue(bRes);
        }

        [TestMethod]
        public async Task Test_Get_Options_Is_Not_Null()
        {
            int electionId = 1;
            var result = await Client.GetOptions(electionId);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Test_Get_Options_Have_Same_Election_Id()
        {
            int electionId = 1;
            var result = await Client.GetOptions(electionId);

            bool bRes = true;
            foreach (var i in result)
            {
                if (i.ElectionId != electionId)
                {
                    bRes = false;
                    break;
                }
            }

            Assert.IsTrue(bRes);
        }

        [TestMethod]
        public async Task Test_Get_User_Vote_Is_Not_Null()
        {
            string username = "000000000";
            int electionId = 1;
            string privateKey = "MIIEpAIBAAKCAQEA0wksOpV2cD66QnDtQmYycUMcJDTFwr1H23jX5RXWZ1v1b0LsZISSRLQRjjM" +
                                "/ogZl2ID8mPVu+W80Tz8YI4z7BWNZ/NkAUDirkQ7vqJhniH29t13TJ1D8qYGbCvVNSMfWKAidvE" +
                                "3Vl+9IrQHDhGEEfhb3sEqfLUFs33U2dtvmluBFpzSs4oDOSTSJjkB/xDG4w+mneG6jGd3QSZkNE" +
                                "t9o8CBn+IlJfgKQnPC5iLrfrhC96D2e/Sim/WZaeacz4jbuq5sE4qlsCus+qJ3fwkjPedRHKhBN" +
                                "NxzCXdAM79zdoqlXRFQCwA/6G1e93qOVCdTX6nELnMROxEds99s03CrcvQIDAQABAoIBAQCUfeX" +
                                "TKwv1mw2wZolrqUFhFNmeWEu25TzEn1k6Zo1AmSIvsaVobSgGk7WumzzxIxNSriAO+NOnS6pBAK" +
                                "JZkX+noOrW0VI5IEcIgLv9o49mKSPu/GPxTDkchIe1xDknNwXnkXh74UCj8cjvg0gxw5pHpp2OP" +
                                "FOYomqbKXyzUbQFAG2hadVi8FltDQFt1835mqp89W+N8M+wI7AbbKpOZmVRKGOtx4n+bSojpcaE" +
                                "4mGLGKmYu9+3RnOm81cPQQ8RFwH6oc5TtkoAbIeFLlV2oqVVq19JecBEzZvjpcIEgZoJ45XxchM" +
                                "FGRQE81CimD279bds3ds9doVp5xzJFQ3LV7S5AoGBAPucVfSjLx2NJAXH7DdruL+1cttOV5TRbL" +
                                "Nme05aTFvX9RCTsLcWwemT7oW+wBXVHQLO8/ipT6vhy1yxkxu0G3qXw8B68IdYh94qieNo2Vsly" +
                                "UYJA+jmRM1G1DupaqKlKB4oba6s3Bgjo5TBlzVqRsF/gWgjYUBl4ScBLbr/+dwbAoGBANa3omd6" +
                                "/Nb/pUTcYkX2NUn6BiT66WcULxJBaLPKMNvHGPBvhS3bAVF7kamFSr/TDibmfqnlc0i7TedZsU9" +
                                "cXJDsxVpWCwBC7Exl11lgFd14n4iw5zWog04M7ScCrRV/nzlepn7CXJzzofcYZwbrweWvC54Jmq" +
                                "TBY+S5H7s1rwgHAoGAOEtfM+/6z6rPf3eekzvHxyTKwOSDVemRX4YzWnF7miT0ULQqmpw94IvXl" +
                                "xb5lSjsZ79z+JOxHqzTOjcEyfL/HuurwXoALNgS9hMgbL/9uZX3tXK47Dfw9ti9DWg3UpKPKkgz" +
                                "WhyU1dXLeLHjqfDQSeTESE96J9Vjjb2GxMsW1TcCgYBKN61GMYfF1RXOSJMbdbATwv1uuOAGhj6" +
                                "DA+LqCmB7B0XCjfmt0P6SqLC/tNgSmCRTI+byWOJRaJTT+/fC470HRyAsSoOA8qD1A1q9hO3p4L" +
                                "DcMbGppXbFeshabJ1hSfzCesn4FLyob7ozS9cI2GLsPmY4mT8/5azeuXKTZkn67wKBgQCxcn4bc" +
                                "hnFs+w4wCfBb4edjJq36Umi4mM95Z004NiPrjZPh4C9hz3zVOl5BWfgXlb++Wutm9qWAdAnIO0e" +
                                "R/CX62tiPsV8dZPUHqoiPxfH3Ypu1DIgnY3TUaBK/PUtkELf47adNAl8KpUmHFZbmF5+gkJp4xw" +
                                "/AyKq2h6+ghnmhg==";
            var result = await Client.GetUserVote(username, electionId, privateKey);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Test_Get_User_Vote_Same_Election_Id()
        {
            string username = "000000000";
            int electionId = 1;
            string privateKey = "MIIEpAIBAAKCAQEA0wksOpV2cD66QnDtQmYycUMcJDTFwr1H23jX5RXWZ1v1b0LsZISSRLQRjjM" +
                                "/ogZl2ID8mPVu+W80Tz8YI4z7BWNZ/NkAUDirkQ7vqJhniH29t13TJ1D8qYGbCvVNSMfWKAidvE" +
                                "3Vl+9IrQHDhGEEfhb3sEqfLUFs33U2dtvmluBFpzSs4oDOSTSJjkB/xDG4w+mneG6jGd3QSZkNE" +
                                "t9o8CBn+IlJfgKQnPC5iLrfrhC96D2e/Sim/WZaeacz4jbuq5sE4qlsCus+qJ3fwkjPedRHKhBN" +
                                "NxzCXdAM79zdoqlXRFQCwA/6G1e93qOVCdTX6nELnMROxEds99s03CrcvQIDAQABAoIBAQCUfeX" +
                                "TKwv1mw2wZolrqUFhFNmeWEu25TzEn1k6Zo1AmSIvsaVobSgGk7WumzzxIxNSriAO+NOnS6pBAK" +
                                "JZkX+noOrW0VI5IEcIgLv9o49mKSPu/GPxTDkchIe1xDknNwXnkXh74UCj8cjvg0gxw5pHpp2OP" +
                                "FOYomqbKXyzUbQFAG2hadVi8FltDQFt1835mqp89W+N8M+wI7AbbKpOZmVRKGOtx4n+bSojpcaE" +
                                "4mGLGKmYu9+3RnOm81cPQQ8RFwH6oc5TtkoAbIeFLlV2oqVVq19JecBEzZvjpcIEgZoJ45XxchM" +
                                "FGRQE81CimD279bds3ds9doVp5xzJFQ3LV7S5AoGBAPucVfSjLx2NJAXH7DdruL+1cttOV5TRbL" +
                                "Nme05aTFvX9RCTsLcWwemT7oW+wBXVHQLO8/ipT6vhy1yxkxu0G3qXw8B68IdYh94qieNo2Vsly" +
                                "UYJA+jmRM1G1DupaqKlKB4oba6s3Bgjo5TBlzVqRsF/gWgjYUBl4ScBLbr/+dwbAoGBANa3omd6" +
                                "/Nb/pUTcYkX2NUn6BiT66WcULxJBaLPKMNvHGPBvhS3bAVF7kamFSr/TDibmfqnlc0i7TedZsU9" +
                                "cXJDsxVpWCwBC7Exl11lgFd14n4iw5zWog04M7ScCrRV/nzlepn7CXJzzofcYZwbrweWvC54Jmq" +
                                "TBY+S5H7s1rwgHAoGAOEtfM+/6z6rPf3eekzvHxyTKwOSDVemRX4YzWnF7miT0ULQqmpw94IvXl" +
                                "xb5lSjsZ79z+JOxHqzTOjcEyfL/HuurwXoALNgS9hMgbL/9uZX3tXK47Dfw9ti9DWg3UpKPKkgz" +
                                "WhyU1dXLeLHjqfDQSeTESE96J9Vjjb2GxMsW1TcCgYBKN61GMYfF1RXOSJMbdbATwv1uuOAGhj6" +
                                "DA+LqCmB7B0XCjfmt0P6SqLC/tNgSmCRTI+byWOJRaJTT+/fC470HRyAsSoOA8qD1A1q9hO3p4L" +
                                "DcMbGppXbFeshabJ1hSfzCesn4FLyob7ozS9cI2GLsPmY4mT8/5azeuXKTZkn67wKBgQCxcn4bc" +
                                "hnFs+w4wCfBb4edjJq36Umi4mM95Z004NiPrjZPh4C9hz3zVOl5BWfgXlb++Wutm9qWAdAnIO0e" +
                                "R/CX62tiPsV8dZPUHqoiPxfH3Ypu1DIgnY3TUaBK/PUtkELf47adNAl8KpUmHFZbmF5+gkJp4xw" +
                                "/AyKq2h6+ghnmhg==";
            var result = await Client.GetUserVote(username, electionId, privateKey);

            bool bRes = result.ElectionId == electionId;

            Assert.IsTrue(bRes);
        }
    }
}
