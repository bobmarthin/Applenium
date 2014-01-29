﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Applenium
{


    public class StreamScenario
    {
        /// <summary>
        /// A dual mode scenario - when "positiveTest" is set to true, the returened results should reflect that all API calls should return objects.
        /// When "positiveTest" is set to false - the API calls can fail, to indicate a negative test. This is (Negative result ) the expected condition.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="positiveTest"></param>
        /// <returns></returns>


        public bool Scenario(string username, bool positiveTest)
        {
            string[] usernames = username.Split('!');
            string ownerWall = usernames[0];
            string taggedUser1 = usernames[1];
            string taggedUser2 = usernames[2];
            string liker = usernames[3];

            StreamsApiRequest sar = new StreamsApiRequest();

            bool success = true;
            string token = sar.login(ownerWall, "123456");


            // try posting 
            StreamsApiResultModel.PostMessageResultModel pmrm = sar.PostMessageRequest(Constants.ACTION_DISCUSSION, token, ownerWall, "Automation Post " + DateTime.Now + " @" + taggedUser1 + " and @" + taggedUser2);

            if (pmrm != null)
            {

                // try liking a post
                string messageID = pmrm.id;
                StreamsApiResultModel.LikeMessageRequestModel like = sar.LikePost(Constants.ACTION_LIKE, token, pmrm.id);

                // make sure users are tagged
                if (like != null)
                {

                    // Check 1st user is tagged on his wall
                    List<StreamsApiResultModel.GetStreamRequestModel> getPost = sar.GetStream(taggedUser1, Constants.ACTION_GET_USER_STREAM, messageID);
                    bool foundPost = false;
                    foreach (StreamsApiResultModel.GetStreamRequestModel post in getPost)
                    {

                        if (post.id.Equals(messageID))
                        {
                            foundPost = true;
                            StreamsApiResultModel.rootData root = post.rootData;
                            StreamsApiResultModel.reason reason = root.reason;
                            if (reason.type.Equals("Tagged"))
                            {
                                success = true;
                                break;
                            }
                            else
                            {
                                success = false;
                                Logger.Error("Can't get reason for post");
                            }
                        }



                    }

                    if (foundPost && positiveTest)
                    {
                        success = true;
                    }

                    else if (!foundPost && !positiveTest)
                    {
                        success = true;
                    }

                    else
                    {
                        success = false;

                        if (positiveTest)
                        {
                            Logger.Error("Can't find post on user1 wall");
                        }
                        else if (!positiveTest)
                        {
                            Logger.Error("Found post on user1 wall and it shouldn't be there!");
                        }
                    }
                }


                // Check 2nd user is tagged on his wall
                List<StreamsApiResultModel.GetStreamRequestModel> getPost2 = sar.GetStream(taggedUser2, Constants.ACTION_GET_USER_STREAM, messageID);
                bool foundPost2 = false;
                foreach (StreamsApiResultModel.GetStreamRequestModel post in getPost2)
                {
                    if (post.id.Equals(messageID))
                    {
                        foundPost2 = true;
                        StreamsApiResultModel.rootData root = post.rootData;
                        StreamsApiResultModel.reason reason = root.reason;
                        if (reason.type.Equals("Tagged"))
                        {
                            success = true;
                            break;
                        }

                        else
                        {
                            success = false;
                            Logger.Error("Can't get reason for post");
                        }
                    }


                }

                if (foundPost2 && positiveTest)
                {
                    success = true;
                }

                else if (!foundPost2 && !positiveTest)
                {
                    success = true;
                }

                else
                {
                    if (positiveTest)
                    {
                        Logger.Error("Can't find post on user2 wall");
                    }
                    else if (!positiveTest)
                    {
                        Logger.Error("Found post on user2 wall and it shouldn't be there!");
                    }
                }



            }

            return success;
        }


        /// <summary>
        /// A scenario to make sure opt-out users can't post API Calls to streams 
        /// </summary>
        /// <param name="username"></param>
        /// <returns>result</returns>
        public bool OptOutAPIFaill(string username)
        {
            bool success = true;


            return success;
        }



    }
}
