﻿namespace Mazda.Model
{
    public class ForgotPass
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string expireTime { get; set; }
        public string UserName { get; set; }
    }
}
