import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Text, TextsClient } from 'src/app/autogenerated/texts-client';

@Component({
  templateUrl: './edit-text.component.html',
  styleUrls: ['./edit-text.component.scss']
})
export class EditTextComponent implements OnInit {

  text$?: Observable<Text>;

  textForm!: FormGroup;

  loading = false;
  success = false;

  textId!: string;

  constructor(private textsClient: TextsClient,
    private route: ActivatedRoute,
    private fb: FormBuilder) { }

  ngOnInit(): void {

    this.textForm = this.fb.group({
      title: [''],
      textContent: [''],
      inputLanguage: [''],
    });

    this.text$ = this.route.paramMap.pipe(
      switchMap(params => {
        this.textId = params.get('id') || '';

        return this.textsClient.getTextById(this.textId);
      })
    );

    this.text$.subscribe(text => {
      this.textForm.setValue({
        title: text.title,
        textContent: text.textContent,
        inputLanguage: text.inputLanguage
      });
    })
  }

  async onSubmit() {

    this.loading = true;

    const formValue = this.textForm.value;

    this.textsClient.updateText(this.textId, new Text({
      id: this.textId,
      inputLanguage: formValue.inputLanguage,
      textContent: formValue.textContent,
      title: formValue.title
    })).subscribe(response => {
      this.loading = false;
      console.log(response)
    }, err => {
      this.success = false;
    });
  }
}
