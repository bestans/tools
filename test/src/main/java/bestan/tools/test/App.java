package bestan.tools.test;

import java.time.Instant;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import bestan.common.log.Glog;

/**
 * Hello world!
 *
 */
public class App 
{
	public static class TestMapValue {
		public int value = 10;
		public int value2 = 102;
		
		public TestMapValue(int v1, int v2) {
			this.value = v1;
			this.value2 = v2;
		}
		
		@Override
		public String toString() {
			// TODO Auto-generated method stub
			return"[value="+value+",value2="+value2+"]";
		}
	}
    public static void main( String[] args )
    {
    	test3();
    }
    
    public static void test3() {
    	Map<String, TestMapValue> mp = new HashMap<>();
    	mp.put("aa", new TestMapValue(111, 222));
    	mp.put("bbb", new TestMapValue(333, 444));
    	
    	List<TestMapValue> ls = new ArrayList<>();
    	ls.add(new TestMapValue(10, 11));
    	ls.add(new TestMapValue(20, 21));
    	ls.add(new TestMapValue(30, 31));
    	System.out.println(mp);
    	System.out.println(ls);
    	
    }
    public static void test1() {
        System.out.println( "Hello World!"  + new Date());
        Date date = new Date(System.currentTimeMillis() + 30 * 60 * 1000);
        Instant instant = date.toInstant();
        System.out.println(instant);
        ZoneId zone = ZoneId.systemDefault();
        System.out.println(zone);
        var localDateTime = LocalDateTime.ofInstant(instant, zone).toLocalDate();
        System.out.println(localDateTime.toEpochDay());
        
        System.out.println(LocalDateTime.ofInstant(date.toInstant(), ZoneId.systemDefault()).toLocalDate().toEpochDay());
    }
    
    public static void test2() {
    	Glog.init();
    	Glog.trace("bbbbbb11111");
    }
}
