package com.onewen.baidupan.util;

import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.TimeUnit;

import okhttp3.ConnectionPool;
import okhttp3.Cookie;
import okhttp3.CookieJar;
import okhttp3.FormBody;
import okhttp3.Headers;
import okhttp3.HttpUrl;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;

/**
 * http访问工具
 *
 * @author 梁光运
 * @date 2018年8月18日
 */
public class HttpUtil {

	public static final MediaType JSON_MEDIA_TYPE = MediaType.parse("application/json; charset=utf-8");

	private static final int DEFAULT_TIMEOUT = 10000;
	private static final int DEFAULT_WRITE_TIMEOUT = 3600000;

	private static final Map<String, Headers> HOST_HEADERS = new HashMap<>();

	private final OkHttpClient httpClient;

	private final CookieStore cookieStore;

	public HttpUtil() {
		this.cookieStore = new CookieStore();
		ConnectionPool pool = new ConnectionPool(32, 30, TimeUnit.MINUTES);
		httpClient = new OkHttpClient.Builder().cookieJar(new CookieJar() {

			@Override
			public void saveFromResponse(HttpUrl url, List<Cookie> cookies) {
				cookieStore.addCookie(url, cookies);
			}

			@Override
			public List<Cookie> loadForRequest(HttpUrl url) {
				List<Cookie> cookies = cookieStore.getCookie(url);
				return (List<Cookie>) (cookies != null ? cookies : new ArrayList<>());
			}
		}).readTimeout(DEFAULT_TIMEOUT * 10, TimeUnit.MILLISECONDS).connectTimeout(DEFAULT_TIMEOUT * 10, TimeUnit.MILLISECONDS)
				.writeTimeout(DEFAULT_WRITE_TIMEOUT, TimeUnit.MILLISECONDS).retryOnConnectionFailure(true)
				.connectionPool(pool).build();
	}

	/**
	 * 发送GET请求
	 * 
	 * @param url     地址
	 * @param headers 头信息
	 * @return response
	 * @throws IOException
	 */
	public Response getResponse(String url, Map<String, String> headers) throws IOException {
		Request.Builder reqBuilder = new Request.Builder();
		if (headers != null && !headers.isEmpty())
			reqBuilder.headers(Headers.of(headers));
		if (headers == null) {
			String host = HttpUrl.parse(url).host();
			if (HOST_HEADERS.containsKey(host))
				reqBuilder.headers(HOST_HEADERS.get(host));
		}
		Request request = reqBuilder.url(url).build();
		Response response = httpClient.newCall(request).execute();
		return response;
	}

	/**
	 * 发送GET请求
	 * 
	 * @param url 地址
	 * @return response
	 * @throws IOException
	 */
	public Response getResponse(String url) throws IOException {
		return getResponse(url, null);
	}

	/**
	 * 发送GET请求
	 * 
	 * @param url     地址
	 * @param headers 头信息
	 * @return string
	 * @throws IOException
	 */
	public String getString(String url, Map<String, String> headers) throws IOException {
		return getResponse(url, headers).body().string();
	}

	/**
	 * 发送GET请求
	 * 
	 * @param url 目标地址
	 * @return
	 * @throws IOException
	 */
	public String getString(String url) throws IOException {
		return getString(url, null);
	}

	/**
	 * 发送POST请求
	 * 
	 * @param url     地址
	 * @param json    数据
	 * @param headers 数据头
	 * @return
	 * @throws IOException
	 */
	public String post(String url, String json, Map<String, String> headers) throws IOException {
		RequestBody body = RequestBody.create(JSON_MEDIA_TYPE, json);
		Request.Builder reqBuilder = new Request.Builder();
		if (headers != null && !headers.isEmpty())
			reqBuilder.headers(Headers.of(headers));
		if (headers == null) {
			String host = HttpUrl.parse(url).host();
			if (HOST_HEADERS.containsKey(host))
				reqBuilder.headers(HOST_HEADERS.get(host));
		}
		Request request = reqBuilder.url(url).post(body).build();
		Response response = httpClient.newCall(request).execute();
		return response.body().string();
	}

	/**
	 * 发送POST请求
	 * 
	 * @param url  地址
	 * @param json 数据
	 * @return
	 * @throws IOException
	 */
	public String post(String url, String json) throws IOException {
		return post(url, json, null);
	}

	/**
	 * post 方法
	 * 
	 * @param url     目标地址
	 * @param form    表单数据
	 * @param headers 数据头
	 * @return
	 * @throws IOException
	 */
	public String post(String url, Map<String, Object> form, Map<String, String> headers) throws IOException {
		FormBody.Builder formbuilder = new FormBody.Builder();
		form.forEach((k, v) -> {
			formbuilder.add(k, v.toString());
		});
		RequestBody body = formbuilder.build();
		Request.Builder reqBuilder = new Request.Builder();
		if (headers != null && !headers.isEmpty())
			reqBuilder.headers(Headers.of(headers));
		if (headers == null) {
			String host = HttpUrl.parse(url).host();
			if (HOST_HEADERS.containsKey(host))
				reqBuilder.headers(HOST_HEADERS.get(host));
		}
		Request request = reqBuilder.url(url).post(body).build();
		Response response = httpClient.newCall(request).execute();
		return response.body().string();
	}

	/**
	 * post 方法
	 * 
	 * @param url     目标地址
	 * @param form    表单数据
	 * @param headers 数据头
	 * @return
	 * @throws IOException
	 */
	public String post(String url, Map<String, Object> form) throws IOException {
		return post(url, form, null);
	}

	/**
	 * post 方法
	 * 
	 * @param url     目标地址
	 * @param bytes   数据
	 * @param headers 数据头
	 * @return
	 * @throws IOException
	 */
	public String post(String url, byte[] bytes, Map<String, String> headers) throws IOException {
		MediaType mediaType = MediaType.parse("application/octet-stream");
		RequestBody.create(mediaType, bytes);
		RequestBody fileBody = RequestBody.create(mediaType, bytes);
		RequestBody requestBody = new MultipartBody.Builder().setType(MultipartBody.FORM)
				.addFormDataPart("file", "blob", fileBody).build();
		Request.Builder reqBuilder = new Request.Builder();
		if (headers != null && !headers.isEmpty())
			reqBuilder.headers(Headers.of(headers));
		if (headers == null) {
			String host = HttpUrl.parse(url).host();
			if (HOST_HEADERS.containsKey(host))
				reqBuilder.headers(HOST_HEADERS.get(host));
		}
		Request request = reqBuilder.url(url).post(requestBody).build();
		Response response = httpClient.newCall(request).execute();
		return response.body().string();
	}

	/**
	 * post 方法
	 * 
	 * @param url     目标地址
	 * @param bytes   数据
	 * @param headers 数据头
	 * @return
	 * @throws IOException
	 */
	public String post(String url, byte[] bytes) throws IOException {
		return post(url, bytes, null);
	}

	/**
	 * OPTIONS 方法
	 * 
	 * @param url     连接地址
	 * @param headers 头部信息
	 * @return
	 * @throws IOException
	 */
	public String options(String url, Map<String, String> headers) throws IOException {
		Request.Builder reqBuilder = new Request.Builder();
		if (headers != null && !headers.isEmpty())
			reqBuilder.headers(Headers.of(headers));
		if (headers == null) {
			String host = HttpUrl.parse(url).host();
			if (HOST_HEADERS.containsKey(host))
				reqBuilder.headers(HOST_HEADERS.get(host));
		}
		Request request = reqBuilder.method("OPTIONS", null).url(url).build();
		Response response = httpClient.newCall(request).execute();
		return response.body().string();
	}

	/**
	 * OPTIONS 方法
	 * 
	 * @param url     连接地址
	 * @param headers 头部信息
	 * @return
	 * @throws IOException
	 */
	public String options(String url) throws IOException {
		return options(url, null);
	}

	/**
	 * 获取cookie管理
	 * 
	 * @return
	 */
	public CookieStore getCookieStore() {
		return cookieStore;
	}

	/**
	 * 获取URL的参数
	 * 
	 * @param url
	 * @return
	 */
	public static Map<String, String> getURLParams(String url) {
		Map<String, String> params = new HashMap<>();
		if (url.indexOf("?") > 0) {
			url = url.substring(url.indexOf("?") + 1, url.length());
		}
		String[] strs = url.split("&");
		for (String str : strs) {
			String[] s = str.split("=");
			if (s.length < 2)
				continue;
			params.put(s[0], s[1]);

		}
		return params;
	}

	/**
	 * 请求头部信息
	 * 
	 * @param url     地址
	 * @param headers 头部信息
	 */
	public static void addHostHeaders(String url, Map<String, String> headers) {
		addHostHeaders(HttpUrl.parse(url), headers);
	}

	/**
	 * 请求头部信息
	 * 
	 * @param url     地址
	 * @param headers 头部信息
	 */
	public static void addHostHeaders(HttpUrl httpUrl, Map<String, String> headers) {
		HOST_HEADERS.put(httpUrl.host(), Headers.of(headers));
	}

}
