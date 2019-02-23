package bestan.tools.mail;

/**
 * Hello world!
 *
 */
public class App 
{
    public static void main( String[] args )
    {
    	if (args.length != 3) {
    		System.out.println("wrong parameters.need mail address, title and content");
    		System.exit(1);
    		return;
    	}
    	boolean ret = MailUtil.sendEmailByFile(args[0], args[1], args[2]);
		System.exit(ret ? 0 : 1);
    }
}
