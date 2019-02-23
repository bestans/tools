package com.onewen.baidupan.service;

import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.RejectedExecutionHandler;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

import com.onewen.baidupan.util.NameThreadFactory;

/**
 * 线程池服务
 * 
 * @author 梁光运
 * @date 2018年9月3日
 */
public class ThreadPoolService {

	private static ThreadPoolService instance;

	public static ThreadPoolService getInstance() {
		if (instance == null)
			instance = new ThreadPoolService();
		return instance;
	}

	private final ExecutorService uploadFilePool = Executors
			.newFixedThreadPool(5, new NameThreadFactory("uploadfile"));

	private final ExecutorService superFilePool = new ThreadPoolExecutor(
			5, 5, 0L,
			TimeUnit.MILLISECONDS, new ArrayBlockingQueue<>(100),
			new NameThreadFactory("superfile"), new RejectedExecutionHandler() {
				@Override
				public void rejectedExecution(Runnable r, ThreadPoolExecutor executor) {
					try {
						executor.getQueue().put(r);
					} catch (InterruptedException e) {
						e.printStackTrace();
					}
				}
			});

	private final ExecutorService downloadFilePool = Executors.newFixedThreadPool(5,
			new NameThreadFactory("downloadfile"));

	public ExecutorService getUploadFilePool() {
		return uploadFilePool;
	}

	public ExecutorService getSuperFilePool() {
		return superFilePool;
	}

	public ExecutorService getDownloadFilePool() {
		return downloadFilePool;
	}

	public void showndown() {
		if (!superFilePool.isShutdown())
			superFilePool.shutdown();
		if (!uploadFilePool.isShutdown())
			uploadFilePool.shutdown();
		if(!downloadFilePool.isShutdown())
			downloadFilePool.shutdown();
	}

}
