import { Component } from "@angular/core";
import { HttpClient } from "@angular/common/http";

@Component({
    selector: 'blob-upload-comp',
    templateUrl: './blob.component.html',
    styleUrls: ['/blob.component.css']
})
export class BlobUploadComponent {
    title: string = "blob sample";
    fileToUpload: File = null;

    constructor(private httpClient: HttpClient) { }

    uploadToBLob(files: FileList) {
        this.fileToUpload = files.item(0);
        const formData: FormData = new FormData();
        formData.append('fileKey', this.fileToUpload, this.fileToUpload.name);
         this.httpClient
            .post("", formData, /*{ headers: yourHeadersConfig }*/)
             .subscribe(event => {

             });
    }

    UploadFile(files: File[], FileName) {
        var formData = new FormData();
        files.forEach(f => formData.append("files", f));
        this.httpClient
            .post("PathToAPI" + FileName, formData)
            .subscribe(event => { });
    }
}
