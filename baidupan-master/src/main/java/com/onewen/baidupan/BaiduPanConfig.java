package com.onewen.baidupan;

import java.util.List;

import bestan.common.lua.BaseLuaConfig;

public class BaiduPanConfig extends BaseLuaConfig {
	public List<AccountConfig> accounts;
	public String yunPath;
	
	public long expiredTime;
	
	public static class AccountConfig extends BaseLuaConfig {
		public String account;
		public String password;
	}
}
