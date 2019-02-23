package com.onewen.baidupan;

import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.Date;
import java.util.List;
import java.util.concurrent.atomic.AtomicInteger;

import org.apache.log4j.PropertyConfigurator;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.common.collect.Lists;
import com.onewen.baidupan.model.Account;
import com.onewen.baidupan.service.BaiduPanService;
import com.onewen.baidupan.service.LoginService;
import com.onewen.baidupan.service.ThreadPoolService;

import bestan.common.lua.LuaConfigs;

/**
 * 百度云盘
 * 
 * @author 梁光运
 * @date 2018年8月23日
 */
public class BaiduPan {

	public static AtomicInteger finish = new AtomicInteger(0);
	private static long interval = 50;
	
	public static final Logger LOGGER = LoggerFactory.getLogger(BaiduPan.class);
	
	public static void main(String[] args) throws Exception {
		PropertyConfigurator.configure("log4j.properties");
		var config = LuaConfigs.loadSingleConfig("config.lua", BaiduPanConfig.class);
		if (null == config) {
			System.out.println("can not find config.lua");
			System.exit(1);
			return;
		}
		
		var startTime = System.currentTimeMillis();
		var date = new Date();
		var days = LocalDateTime.ofInstant(date.toInstant(), ZoneId.systemDefault()).toLocalDate().toEpochDay();
		String serverPath = config.yunPath + ((days % 2 == 0) ? "backup1/" : "backup2/");
		List<Account> accounts = Lists.newArrayList();
		// 登陆
		for (var acfg : config.accounts) {
			Account account = LoginService.getInstance().startLogin(acfg.account, acfg.password);
			if (account == null)
			{
				System.out.println("login account " + acfg.account + " failed");
				System.exit(1);
				return;
			}
			accounts.add(account);
		}
		String[] xxargs = {
			"account-bestan.json",	
		};
		var importPaths = args;
		if (importPaths.length <= 0) {
			LOGGER.error("failed.reason=empty paths");
			System.exit(1);
			return;
		}
		List<String> deleteList = Lists.newArrayList();
		List<String> fileNames = Lists.newArrayList();
		for (var path : importPaths) {
			var paths = path.split("/");
			if (paths == null || paths.length <= 0) {
				System.out.println("error path when split,path=" + path);
				System.exit(1);
				return;
			}
			var fileName = paths[paths.length - 1];
			deleteList.add(serverPath + fileName);
			fileNames.add(fileName);
		}
		final long expiredTime = config.expiredTime;
		boolean ret = true;
		for (var account : accounts) {
			//删除旧文件
			BaiduPanService.getInstance().deleteFile(account, deleteList);
			Thread.sleep(1000);
			for (int i = 0; i < importPaths.length; ++i) {
				String path = importPaths[i];
				var fileName = fileNames.get(i);
				finish.set(0);
				long curExpiredTime = 0;
				
				BaiduPanService.getInstance().uplaodFileOrDir(account, path, serverPath + fileName + "/");

				while (finish.get() < 1) {
					if (curExpiredTime >= expiredTime) {
						ret = false;
						break;
					}
					Thread.sleep(interval);
					curExpiredTime += interval;
				}

				if (!ret) {
					LOGGER.debug("uplaodFile timeout,path={}", path);
					break;
				}
			}
		}
		
		ThreadPoolService.getInstance().showndown();

		LOGGER.debug("cost time（毫秒） = {}", System.currentTimeMillis() - startTime);
		System.exit(ret ? 0 : 1);
//		List<PanFile> panFiles = BaiduPanService.getInstance().listFile(account, "/");
//		for (PanFile panFile : panFiles) {
//			if(!panFile.isIsdir()) {
//				System.out.println("xxx=" + panFile.getPath());
//				//BaiduPanService.getInstance().downloadFile(account, panFile, "F:\\BaiduYunDownload");
//				LOGGER.info(panFile.getServer_filename());
//			}
//		}
	}
}
