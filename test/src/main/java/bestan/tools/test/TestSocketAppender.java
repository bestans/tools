package bestan.tools.test;

import java.net.InetAddress;

import org.apache.log4j.helpers.OnlyOnceErrorHandler;
import org.apache.log4j.net.SocketAppender;
import org.apache.log4j.spi.LoggingEvent;

import bestan.common.log.Glog;

public class TestSocketAppender extends SocketAppender {

	public TestSocketAppender() {
		errorHandler = new MyError();
	}

	public TestSocketAppender(InetAddress address, int port) {
		super(address, port);
		errorHandler = new MyError();

	}

	/**
	 * Connects to remote server at <code>host</code> and <code>port</code>.
	 */
	public TestSocketAppender(String host, int port) {
		super(host, port);
		errorHandler = new MyError();
	}

	public class MyError extends OnlyOnceErrorHandler {
		@Override
		public void error(String message) {
			// TODO Auto-generated method stub
			super.error(message);
			onFailed(message);
		}

		@Override
		public void error(String message, Exception e, int errorCode) {
			// TODO Auto-generated method stub
			super.error(message, e, errorCode);
			onFailed(message);

		}

		@Override
		public void error(String message, Exception e, int errorCode, LoggingEvent event) {
			// TODO Auto-generated method stub
			super.error(message, e, errorCode, event);
			onFailed(message);
		}

		public void onFailed(String message) {
			Glog.error("eeeeroro={}", message);
		}
	}
}
