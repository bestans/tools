package bestan.tools.mail;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;

import org.apache.commons.mail.HtmlEmail;

public class MailUtil {
	public static String readFile(String path) throws IOException {
        File file = new File(path);//定义一个file对象，用来初始化FileReader
        FileReader reader = new FileReader(file);//定义一个fileReader对象，用来初始化BufferedReader
        BufferedReader bReader = new BufferedReader(reader);//new一个BufferedReader对象，将文件内容读取到缓存
        StringBuilder sb = new StringBuilder();//定义一个字符串缓存，将字符串存放缓存中
        String s = "";
        while ((s =bReader.readLine()) != null) {//逐行读取文件内容，不读取换行符和末尾的空格
            sb.append(s + "\n");//将读取的字符串添加换行符后累加存放在缓存中
        }
        bReader.close();
        return sb.toString();
	}
	
	//邮箱验证码
	public static boolean sendEmailByFile(String emailaddress, String title, String file){
		try {
			HtmlEmail email = new HtmlEmail();//不用更改
			email.setHostName("smtp.163.com");//邮箱类型
			//email.setSmtpPort(465);
			email.setCharset("UTF-8");
			email.addTo(emailaddress);// 收件地址
 
			email.setFrom("youhuanye@163.com");//此处填邮箱地址和用户名,用户名可以任意填写
 
			email.setAuthentication("youhuanye@163.com", "tianya1988188"); //此处填写邮箱地址和客户端授权码
 
			email.setSubject(title);//此处填写邮件名，邮件名可任意填写
			email.setMsg(readFile(file));//此处填写邮件内容
			email.setSSL(true);
			email.send();
			return true;
		}
		catch(Exception e){
			e.printStackTrace();
			return false;
		}
	}
	//邮箱验证码
	public static boolean sendEmail(String emailaddress, String title, String msg){
		try {
			HtmlEmail email = new HtmlEmail();//不用更改
			email.setHostName("smtp.163.com");//邮箱类型
			//email.setSmtpPort(465);
			email.setCharset("UTF-8");
			email.addTo(emailaddress);// 收件地址
 
			email.setFrom("youhuanye@163.com");//此处填邮箱地址和用户名,用户名可以任意填写
 
			email.setAuthentication("youhuanye@163.com", "tianya1988188"); //此处填写邮箱地址和客户端授权码
 
			email.setSubject(title);//此处填写邮件名，邮件名可任意填写
			email.setMsg(msg);//此处填写邮件内容
			email.setSSL(true);
			email.send();
			return true;
		}
		catch(Exception e){
			e.printStackTrace();
			return false;
		}
	}
}
